using FontStashSharp;
using FontStashSharp.RichText;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Input;
using System.Diagnostics;

namespace Fage.Runtime.Scenes.Main.Text;

/// <summary>
/// 文本组件。
/// </summary>
/// <remarks>
/// 这个组件绘制剧情文本和可选的角色姓名。文本字体为动态加载的TrueType字体，背景为可更换的图片。
/// </remarks>
public class CurrentTextAreaComponent : IDisposable
{
	protected readonly RichTextLayout LayoutEngine;

	protected SpriteBatch SpriteBatch = null!;

	public Game RootGame { get; }

	public ContentManager Content { get; }

	/// <summary>
	/// 控制呈现的文本样式的一组选项
	/// </summary>
	public TextPresentingOptions TextPresentingOptions { get; }

	public event Action<CurrentTextAreaComponent>? OnParagraphCompleted;
	public event Action<CurrentTextAreaComponent>? OnLineCompleted;

	public ParagraphTypewriterEffect TypewriterEffect { get; }

	public TextAreaWriter Writer { get; }

	public bool ParagraphCompleted => TypewriterEffect.ParagraphCompleted;

	public Texture2D? Background { get; set; } = null!;

	public CurrentTextLayout Layout;

	/// <summary>
	/// 文本框绘制区域
	/// </summary>
	/// <remarks>
	/// 可以通过<see cref="Layout"/>调整边距，来间接调整绘制区域。
	/// </remarks>
	public Rectangle DrawBounds;

	private bool disposedValue;

	public CurrentTextAreaComponent(Game game, ContentManager contents)
	{
		RootGame = game;
		Content = contents;
		TextPresentingOptions = new(game);
		TypewriterEffect = new();
		Writer = new(TypewriterEffect);
		LayoutEngine = new RichTextLayout(new RichTextSettings { FontResolver = TryResolveFont })
		{
			SupportsCommands = false,
			Text = string.Empty
		};
	}

	private SpriteFontBase TryResolveFont(string name)
	{
		Debug.WriteLineIf(!TextPresentingOptions.FontFileNames.Contains(name), $"[警告] 文本组件添加了这些字体：{TextPresentingOptions.FontNames}" +
			$"但是渲染器正在获取字体：\"{name}\"！");

		return TextPresentingOptions.Font;
	}

	#region 绘制参数
	public int Height { get => DrawBounds.Height; set => DrawBounds.Height = value; }
	public int MarginLeft
	{
		get => DrawBounds.X;
		private set => DrawBounds.X = value;
	}

	public float MarginLeftPercent
	{
		get => MarginLeft / RootGame.GraphicsDevice.Viewport.Width;
		private set => DrawBounds.X = (int)MathF.Round(value * RootGame.GraphicsDevice.Viewport.Width);
	}

	public int MarginRight
	{
		get => RootGame.GraphicsDevice.Viewport.Width - DrawBounds.Right;
		set => DrawBounds.Width = RootGame.GraphicsDevice.Viewport.Width - DrawBounds.X - value;
	}

	public float MarginRightPercent
	{
		get => MarginRight / RootGame.GraphicsDevice.Viewport.Width;
		private set => DrawBounds.X = (int)MathF.Round(value * RootGame.GraphicsDevice.Viewport.Width);
	}

	public int MarginBottom
	{
		get => RootGame.GraphicsDevice.Viewport.Height - DrawBounds.Bottom;
		private set => DrawBounds.Y = RootGame.GraphicsDevice.Viewport.Height - DrawBounds.Height - value;
	}

	public float MarginBottomPercent
	{
		get => MarginBottom / RootGame.GraphicsDevice.Viewport.Height;
		private set
		{
			int marginPixels = (int)MathF.Round(value * RootGame.GraphicsDevice.Viewport.Height);
			MarginBottom = marginPixels;
		}
	}

	/// <summary>
	/// 绘制文本的区域
	/// </summary>
	public Rectangle TextDrawingBounds => new(
		DrawBounds.X + Layout.TextPaddingLeft,
		DrawBounds.Y + Layout.TextPaddingTop,
		DrawBounds.Width - Layout.TextPaddingRight,
		DrawBounds.Height);

	public Vector2 TextPosition => new(DrawBounds.X + Layout.TextPaddingLeft, DrawBounds.Y + Layout.TextPaddingTop);
	public int TextAreaWidth => DrawBounds.Width - Layout.TextPaddingRight;

	#endregion

	/// <devdoc>
	/// <summary>
	/// 根据外边距和高度计算并设置大小。
	/// </summary>
	/// <param name="marginLeft"></param>
	/// <param name="marginRight"></param>
	/// <param name="marginBottom"></param>
	/// <param name="height"></param>
	/// </devdoc>
	private void InitializeBoundsByMargin(int marginLeft, int marginRight, int marginBottom, int? height = null)
	{
		Height = height ?? 100;

		MarginLeft = marginLeft;
		MarginRight = marginRight;
		MarginBottom = marginBottom;
	}

	/// <summary>
	/// 每当重置图形设备或初始化时都需要调用一次的方法。
	/// </summary>
	/// <remarks>
	/// 这个方法会重置绘制位置、大小、文本区域宽度等绘制状态。
	/// </remarks>
	public void OnDeviceReset()
	{
		if (Background != null && Layout.Height == 0)
		{
			Layout.Height = Background.Height;
		}

		InitializeBoundsByMargin(Layout.MarginLeft, Layout.MarginRight, Layout.MarginBottom, Layout.Height);
		LayoutEngine.Width = TextAreaWidth;
	}

	public void LoadContent()
	{
		// TODO 可以考虑用素材拼一张背景
		Background = Content.Load<Texture2D>("CurrentTextBackground");
		InitializeBoundsByMargin(Layout.MarginLeft, Layout.MarginRight, Layout.MarginBottom, Background.Height);
		ResetLayoutEngine();
		TextPresentingOptions.ApplyTextSpeed();
	}

	public void ResetLayoutEngine()
	{
		TextPresentingOptions.UpdateFonts();
		LayoutEngine.Font = TextPresentingOptions.Font;
	}

	public void UnloadContent()
	{
		TextPresentingOptions.UnloadResources();
	}

	public void Update(GameTime gameTime)
	{
		MouseStateExtended mState = MouseExtended.GetState();
		KeyboardStateExtended kState = KeyboardExtended.GetState();

		if (RootGame.IsActive
			&& (
				RootGame.Window.ClientBounds.Contains(mState.Position)
					&& mState.WasButtonJustUp(MouseButton.Left)
				|| kState.WasKeyJustUp(Keys.Enter))
			)
		{
			// 没跑完，点了一下
			if (!TypewriterEffect.CurrentLineCompleted)
			{
				// 那就是跳过文本效果的意思
				TypewriterEffect.SkipCurrentLine();
				LayoutEngine.Text = TypewriterEffect.ParagraphText[0..TypewriterEffect.LastPosition].ToString();
				return;
			}

			// 正经地跑完语句
			if (!ParagraphCompleted)
			{
				TypewriterEffect.StartNextLine();
				OnLineCompleted?.Invoke(this);
			}
			else
			{
				OnParagraphCompleted?.Invoke(this);
			}
		}

		if (TypewriterEffect.TryUpdateTextProgress(TextPresentingOptions.TextSpeedInterval, TextPresentingOptions.Font))
		{
			LayoutEngine.Text = TypewriterEffect.ParagraphText[0..TypewriterEffect.LastPosition].ToString();
		}
	}

	public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
	{
		if (Background != null)
			spriteBatch.Draw(Background, DrawBounds, Color.White);

		var cachedPosition = TextPosition;

		LayoutEngine.Draw(spriteBatch, cachedPosition, TextPresentingOptions.TextColor);

		if (TypewriterEffect.CurrentLineCompleted || ParagraphCompleted)
		{
			var lines = LayoutEngine.Lines;
			var lastLine = lines.LastOrDefault() ?? new TextLine();
			float markerY = cachedPosition.Y + lastLine.Size.Y * (lines.Count - 1);
			float markerX = cachedPosition.X + lastLine.Size.X + 2;

			DrawEndMarker(gameTime, spriteBatch, markerX, markerY, ParagraphCompleted); 
		}
	}

	protected virtual void DrawEndMarker(GameTime gameTime, SpriteBatch spriteBatch, float markerX, float markerY, bool isParagraphCompleted)
	{
		Vector2 markerSize = TextPresentingOptions.Font.MeasureString(TextPresentingOptions.EndMarkerText);

		const float Rotation = MathF.PI / 2;

		if (isParagraphCompleted)
			TextPresentingOptions.Font.DrawText(
				spriteBatch,
				TextPresentingOptions.EndMarkerText,
				new Vector2(markerX + markerSize.Y, markerY),
				TextPresentingOptions.MarkerColor,
				Rotation
				);
		else
			TextPresentingOptions.Font.DrawText(
				spriteBatch,
				TextPresentingOptions.EndMarkerText,
				new Vector2(markerX, markerY),
				TextPresentingOptions.MarkerColor
			);

	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				UnloadContent();
			}
			disposedValue = true;
		}
	}

	public void Dispose()
	{
		// 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}