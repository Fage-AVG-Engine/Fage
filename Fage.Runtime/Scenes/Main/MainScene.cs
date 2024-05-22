using Fage.Runtime.Layering;
using Fage.Runtime.Scenes.Main.Branch;
using Fage.Runtime.Scenes.Main.Characters;
using Fage.Runtime.Scenes.Main.Text;
using Fage.Runtime.Script;
using Fage.Runtime.Utility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Neo.IronLua;
using System.Diagnostics;

namespace Fage.Runtime.Scenes.Main;

public class MainScene : CompositeLayerBasedScene
{
	internal readonly ContentManager Content;

	private Texture2D? _background;

	private bool _readyForNextInstruction = true;

	public const string ContentScopePath = "MainScene";

	private readonly SingleTextureLayer BackgroundLayer = new("main bg");

	public Texture2D? Background
	{
		get => _background;
		set
		{
			_background = value;

			if (_background != null)
				ApplyNewBackground();
		}
	}

	public Color FallbackBackgroundColor { get; set; } = Color.Black;

	public Rectangle BackgroundSourceBounds 
	{ 
		get => BackgroundLayer.TextureSourceBounds;
		set => BackgroundLayer.TextureSourceBounds = value;
	}

	public FageScript Script { get; set; } = null!;

	public CurrentTextAreaComponent TextArea { get; }

	public CharactersManager Characters { get; private set; }

	public ScriptingEnvironment ScriptingEnvironment { get; internal set; } = null!;

	public BranchOptionPanel BranchOptionPanel { get; private set; } = null!;

	public MainScene(FageTemplateGame game) : base("main", game)
	{
		Content = new ContentManager(game.Services, Path.Combine(game.Content.RootDirectory, ContentScopePath));
		TextArea = new(game, this, Content)
		{
			Layout = new CurrentTextLayout
			{
				TextPaddingLeft = 10,
				TextPaddingRight = 10,
				TextPaddingTop = 12,
				MarginLeft = 10,
				MarginRight = 10,
				MarginBottom = 26,
			}
		};
		Characters = new(game);
	}

	public override void Initialize()
	{
		base.Initialize();
	}

	private void OnDeviceReset(object? sender, EventArgs e)
	{
		if (Background != null)
		{
			ApplyNewBackground();
		}

		TextArea.OnDeviceReset();
		BranchOptionPanel.PanelAvailableArea = new(0, 0, TextArea.DrawBounds.Width, TextArea.DrawBounds.Y);
		Characters.Layout.Baseline = GraphicsDevice.Viewport.Height * 3 / 10;
	}

	/// <summary>
	/// 计算背景图的相关信息，以完成背景图的更改。
	/// </summary>
	/// <remarks>
	/// 每当更换背景或图形设备重置之后，应该调用一次。
	/// </remarks>
	public void ApplyNewBackground()
	{
		Debug.Assert(Background != null, "在背景加载前调用了" + nameof(ApplyNewBackground));

		BackgroundLayer.Texture = Background;
		BackgroundLayer.DestinationBounds = GraphicsDevice.Viewport.Bounds;
		BackgroundSourceBounds = AdaptBounds.FillDestinationByCenter(Background.Bounds, GraphicsDevice.Viewport.Bounds);
	}

	public override void LoadContent()
	{
		GraphicsDevice.DeviceReset += OnDeviceReset;

		Characters = new(Game);
		BranchOptionPanel = new(Content, Game, this, DiServices.GetRequiredService<IOptions<BranchPanelOptions>>());

		TextArea.LoadContent();
		Characters.LoadContent();
		BranchOptionPanel.PreloadResource();

		// TODO RootLayer.AddBehind(null, UIControlsContainer);
		RootLayer.AddBehind(null, BranchOptionPanel.EffectiveLayer);
		// TODO RootLayer.AddBehind null BacklogLayer
		RootLayer.AddBehind(null, Characters);
		RootLayer.AddBehind(null, TextArea);
		RootLayer.AddBehind(null, BackgroundLayer);

		OnDeviceReset(null, EventArgs.Empty); // 反正第二个参数用不到

		// TextArea.OnParagraphCompleted += BlockingInstructionCompleted;
		// TextArea.OnLineCompleted += BlockingInstructionCompleted;
	}

	protected override void OnScreenSleep()
	{
		Game.AudioManager.BackgroundMusicChannel.SwitchToNextBgmImmediately(null);
		ScriptingEnvironment.Lua.LuaVM.Clear();
		ScriptingEnvironment.Lua.GlobalEnvironment.Clear();
		base.OnScreenSleep();
	}

	public override void Update(GameTime gameTime)
	{
		SetupInitialContentIfNecessary();

		UpdateScript();

		UpdateRootLayer(gameTime);

		BranchOptionPanel.CleanupIfInactive();

		base.Update(gameTime);
	}

	protected override void SetupInitialContent()
	{
#if DEBUG
		Debug.Assert(Script != null, "未加载脚本");
#endif
	}

	private void UpdateScript()
	{
		while (_readyForNextInstruction && Script.MoveNext())
		{
			var current = Script.Current;
			current.Handle(ScriptingEnvironment);

			if (current.BlocksExecution)
			{
				_readyForNextInstruction = false;
				break;
			}
		}
	}

	public override void UnloadContent()
	{
		TextArea.UnloadContent();
		BranchOptionPanel.UnloadResource();
		GraphicsDevice.DeviceReset -= OnDeviceReset;
	}

	public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
	{
		RootLayer.Draw(gameTime, spriteBatch);
	}
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			TextArea.Dispose();
		}
		base.Dispose(disposing);
	}

	internal void InternalCreateScriptingEnvironment()
	{
		CreateScriptingEnvironment();
	}

	protected virtual void CreateScriptingEnvironment()
	{
		Lua luaVM = Game.LuaVM;
		luaVM.Clear();

		LuaGlobal luaGlobal = luaVM.CreateEnvironment();
		LuaEnvironment luaEnv = new(luaVM, luaGlobal);

		ScriptingEnvironment = new ScriptingEnvironment(Script, Game.AudioManager, this, Game, luaEnv);

		luaEnv.ConfigureGlobalInternal(ScriptingEnvironment);
	}

	public void BlockScriptExecution()
	{
		_readyForNextInstruction = false;
	}

	public void UnblockScriptExecution()
	{
		_readyForNextInstruction = true;
	}
}