using FontStashSharp;
using FontStashSharp.RichText;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Input;
using System.Diagnostics;
using Fage.Runtime.Layering;

namespace Fage.Runtime.Scenes.Main.Text;

/// <summary>
/// 文本组件。
/// </summary>
/// <remarks>
/// 这个组件绘制剧情文本和可选的角色姓名。文本字体为动态加载的TrueType字体，背景为可更换的图片。
/// </remarks>
public class CurrentTextAreaComponent : 
	ILayer, 
	ILayeredMouseHandler, ILayeredKeyboardHandler,
	IDisposable
{
	protected readonly MainScene mainScene;
	protected readonly RichTextLayout LayoutEngine;

	protected SpriteBatch SpriteBatch = null!;

	public Game RootGame { get; }

	public ContentManager Content { get; }

	/// <summary>
	/// 控制呈现的文本样式的一组选项
	/// </summary>
	public TextPresentingOptions TextPresentingOptions { get; }

	[Obsolete]
	public event Action<CurrentTextAreaComponent>? OnParagraphCompleted;
	[Obsolete]
	public event Action<CurrentTextAreaComponent>? OnLineCompleted;

	public ParagraphTypewriterEffect TypewriterEffect { get; }

	public TextAreaWriter Writer { get; }

	public bool ParagraphCompleted => TypewriterEffect.ParagraphCompleted;

	internal Texture2D? Background { get => BackgroundLayer.Texture; set => BackgroundLayer.Texture = value; }

	public CurrentTextLayout Layout;

	public SingleTextureLayer BackgroundLayer { get; private set; } = new("text area background");

	#region 图层
	public string Name { get; } = "text area";
	public ILayer? Parent { get; set; }

	#endregion

	/// <summary>
	/// 文本框绘制区域
	/// </summary>
	/// <remarks>
	/// 可以通过<see cref="Layout"/>调整边距，来间接调整绘制区域。
	/// </remarks>
	public ref Rectangle DrawBounds => ref BackgroundLayer.DestinationBounds;

	private bool disposedValue;
	private bool _shouldFetchNext;

	public CurrentTextAreaComponent(Game game, MainScene scene, ContentManager contents)
	{
		mainScene = scene;
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
		if (_shouldFetchNext && FetchNextText())
		{
			mainScene.UnblockScriptExecution();
		}
		_shouldFetchNext = false;

		if (TypewriterEffect.TryUpdateTextProgress(TextPresentingOptions.TextSpeedInterval, TextPresentingOptions.Font))
		{
			LayoutEngine.Text = TypewriterEffect.ParagraphText[0..TypewriterEffect.LastPosition].ToString();
		}
	}

	public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
	{
		BackgroundLayer.Draw(gameTime, spriteBatch);

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

	/// <summary>
	/// 
	/// </summary>
	/// <remarks>
	/// 感觉这个名字不是很好
	/// </remarks>
	/// <returns>返回<see langword="true"/>时，表示应当开始下一段落。
	/// 返回<see langword="false"/>时，表示已提前呈现一行的全部文本。</returns>
	private bool FetchNextText()
	{
		// 没跑完，点了一下
		if (!TypewriterEffect.CurrentLineCompleted)
		{
			// 那就是跳过文本效果的意思
			TypewriterEffect.SkipCurrentLine();
			LayoutEngine.Text = TypewriterEffect.ParagraphText.ToString();
			return false;
		}

		// 一行（或段落）跑完，收到输入
		if (!ParagraphCompleted)
		{
			// 段落未完成，还有其它行，开始下一行
			TypewriterEffect.StartNextLine();
		}
		
		return true;
	}

	bool ILayeredInputHandler<LayeredMouseEventArgs>.HandleInput(ILayer sender, LayeredMouseEventArgs e)
	{
		if (e.IsUserInGame && e.State.WasButtonJustUp(MouseButton.Left))
		{
			_shouldFetchNext = true;
			return true;
		}

		return false;
	}

	bool ILayeredInputHandler<LayeredKeyboardEventArgs>.HandleInput(ILayer sender, LayeredKeyboardEventArgs e)
	{
		if (e.IsGameActive && e.State.WasKeyJustUp(Keys.Enter))
		{
			_shouldFetchNext = true;
			return true;
		}

		return false;
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