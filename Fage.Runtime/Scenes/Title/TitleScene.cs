using Fage.Runtime.Layering;
using Fage.Runtime.UI;
using Fage.Runtime.Utility;
using FontStashSharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Fage.Runtime.Scenes.Title;

public class TitleScene(FageTemplateGame game) : CompositeLayerBasedScene("title root", game)
{
	private Texture2D _backgroundTexture = null!;
	private bool _controlsInitialized = false;

	protected ImageButtonTemplate ButtonTemplate = null!;

	protected readonly ContentManager Content = new(game.Content.ServiceProvider, Path.Combine(game.Content.RootDirectory, "Title"));
	protected readonly SingleTextureLayer BackgroundLayer = new("background");
	protected readonly CompositeLayer ControlsLayer = new("controls");

	protected TitleSceneConfiguration Configuration { get; set; } = null!;

	public const string TitleConfigurationSection = "FageStartup:TitleScreen";

	protected DynamicSpriteFont ButtonTextFont { get; set; } = null!;
	protected float ButtonTextFontBaseSize { get; set; } = 48;
	protected float ButtonTextScaleFactor { get; set; } = 0.5f;
	protected float ButtonTextFontSize => ButtonTextFontBaseSize * ButtonTextScaleFactor;

	protected ImageBasedButton? StartButton = null;
	protected ImageBasedButton? LoadButton = null;
	protected ImageBasedButton? SettingsButton = null;
	protected ImageBasedButton? ExitButton = null;

	public Rectangle SceneDestinationBounds { get; set; }
	public string BackgroundTextureName => Configuration.TitleBackgroundTextureName;

	protected Texture2D BackgroundTexture
	{
		get => _backgroundTexture;
		set
		{
			BackgroundLayer.Texture = _backgroundTexture = value;
			BackgroundLayer.DestinationBounds = AdaptBounds.FillDestinationByCenter(value.Bounds, SceneDestinationBounds);
		}
	}

	public override sealed void Draw(GameTime gameTime, SpriteBatch spriteBatch)
	{
		RootLayer.Draw(gameTime, spriteBatch);
	}

	public override void Update(GameTime gameTime)
	{
		UpdateRootLayer(gameTime);
		base.Update(gameTime);
	}

	/// <summary>
	/// 初始化UI控件
	/// </summary>
	/// <remarks>
	/// 这个方法可以用于创建控件、加载控件资源、将事件绑定到控件。完成上述初始化操作后，可将控件添加到<paramref name="controlsLayer"/>。不需要在这里进行布局操作。
	/// </remarks>
	/// <param name="controlsLayer">放置控件的图层</param>
	[MemberNotNull(nameof(ExitButton), nameof(StartButton), nameof(LoadButton), nameof(SettingsButton))]
	protected virtual void InitializeControls(CompositeLayer controlsLayer)
	{
		if (_controlsInitialized)
		{
			Debug.Assert(ExitButton != null
				&& StartButton != null
				&& LoadButton != null
				&& SettingsButton != null);

			return;
		}

		StartButton = ButtonTemplate.CreateWithText("start", "开始演示", Content);
		LoadButton = ButtonTemplate.CreateWithText("load", "读取存档", Content);
		LoadButton.IsDisabled = true;
		SettingsButton = ButtonTemplate.CreateWithText("settings", "系统设置", Content);
		SettingsButton.IsDisabled = true;
		ExitButton = ButtonTemplate.CreateWithText("exit", "退出系统", Content);

		StartButton.LoadResource();
		LoadButton.LoadResource();
		SettingsButton.LoadResource();
		ExitButton.LoadResource();

		StartButton.Clicked += StartNewGame;
		ExitButton.Clicked += ExitClicked;

		controlsLayer.AddBehind(null, ExitButton);
		controlsLayer.AddBehind(ExitButton.Name, SettingsButton);
		controlsLayer.AddBehind(SettingsButton.Name, LoadButton);
		controlsLayer.AddBehind(LoadButton.Name, StartButton);
	}

	private void ExitClicked(ImageBasedButton obj)
	{
		Game.Exit();
	}

	private void StartNewGame(ImageBasedButton obj)
	{
		Game.SwitchToMainScene(Configuration.NewGame.InitialScript);
	}

	/// <summary>
	/// 对控件进行一个局的布
	/// </summary>
	/// <remarks>
	/// 初始化控件之后会调用一次，以后每次重置图形设备也会调用一次。
	/// </remarks>
	/// <param name="sceneDestinationBounds">整个 scene 的绘制区域</param>
	protected virtual void LayoutControls(Rectangle sceneDestinationBounds)
	{
		float buttonsTopRatio = 206f / 1080f;
		float buttonsLeftRatio = 1200f / 1920f;

		int x = (int)(sceneDestinationBounds.Width * buttonsLeftRatio) + sceneDestinationBounds.X;
		int y = (int)(sceneDestinationBounds.Height * buttonsTopRatio) + sceneDestinationBounds.Y;

		int w = sceneDestinationBounds.Right - x;
		int h = sceneDestinationBounds.Bottom - y;

		Debug.Assert(StartButton != null
			&& LoadButton != null
			&& SettingsButton != null
			&& ExitButton != null, "未创建控件？");

		Point[] sizes = [StartButton.ButtonSize, LoadButton.ButtonSize, SettingsButton.ButtonSize, ExitButton.ButtonSize];
		Rectangle[] buttonBounds = Layout.VerticalStackJustifyEvenly(new(x, y, w, h), sizes);

		Span<Rectangle> boundsInSpan = buttonBounds;
		foreach (ref Rectangle rect in boundsInSpan)
			Layout.HorizontalAlignCenter(ref rect, w);

		StartButton.DestinationArea = buttonBounds[0];
		LoadButton.DestinationArea = buttonBounds[1];
		SettingsButton.DestinationArea = buttonBounds[2];
		ExitButton.DestinationArea = buttonBounds[3];
	}

	public override void LoadContent()
	{
		Configuration = Game.Configuration.GetSection(TitleConfigurationSection).Get<TitleSceneConfiguration>()
			?? throw new InvalidConfigurationException(TitleConfigurationSection, "必须配置标题界面，如不需要标题，请修改源代码。");

		SceneDestinationBounds = Game.GraphicsDevice.Viewport.Bounds;
		Game.GraphicsDevice.DeviceReset += OnDeviceReset;
		ButtonTextFont ??= Game.GenericFontFamily.GetFont(ButtonTextFontSize);

		BackgroundTexture = Content.Load<Texture2D>(BackgroundTextureName);

		ButtonTemplate = new("btn-released", "btn-pressed", "btn-hover")
		{
			Font = ButtonTextFont,
			DisabledTextureName = "btn-disabled",
			ReleasedTextColor = new(239, 239, 239),
			PressedTextColor = new(35, 35, 35),
			HoverTextColor = new(36, 36, 36),
			DisabledTextColor = new(60, 60, 60)
		};

		InitializeControls(ControlsLayer);
		OnDeviceReset(null, null!);
		LayoutControls(SceneDestinationBounds);
		_controlsInitialized = true;

		RootLayer.AddBehind(null, ControlsLayer);
		RootLayer.AddBehind(ControlsLayer.Name, BackgroundLayer);
	}

	public override void UnloadContent()
	{
		Content.UnloadAsset(BackgroundTextureName);

		StartButton!.UnloadResource();
		LoadButton!.UnloadResource();
		SettingsButton!.UnloadResource();
		ExitButton!.UnloadResource();

		Game.GraphicsDevice.DeviceReset -= OnDeviceReset;
	}

	private void OnDeviceReset(object? sender, EventArgs e)
	{
		SceneDestinationBounds = Game.GraphicsDevice.Viewport.Bounds;

		if (BackgroundLayer.Texture is not null)
		{
			BackgroundLayer.DestinationBounds = SceneDestinationBounds;
			BackgroundLayer.TextureSourceBounds = AdaptBounds.FillDestinationByCenter(BackgroundLayer.Texture.Bounds, SceneDestinationBounds);
		}

		if (_controlsInitialized)
		{
			LayoutControls(SceneDestinationBounds);
		}
	}

}
