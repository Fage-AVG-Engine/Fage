using Fage.Runtime.Audio;
using Fage.Runtime.Scenes;
using Fage.Runtime.Scenes.Main;
using Fage.Runtime.Scenes.Main.Branch;
using Fage.Runtime.Scenes.Splash;
using Fage.Runtime.Scenes.Title;
using Fage.Runtime.Script;
using Fage.Runtime.Utility;
using FontStashSharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input.InputListeners;
using Neo.IronLua;
using System.Diagnostics;
using System.Text;

namespace Fage.Runtime
{
	public partial class FageTemplateGame : Game
	{
		private SpriteBatch _spriteBatch = null!;
		private readonly ConfigurationManager configurationManager;

		protected readonly GraphicsDeviceManager GraphicsManager;


		private Task? _mainSceneInitializeTask;
		private Task? _titleSceneInitializeTask;

		private Task<int?>? _alertMessageTask;
		private Tuple<string, Exception>? _criticalErrorInfo;

		private Task? _switchingSceneTask;

		/// <summary>
		/// 默认字体，包含Basic Latin和Enclosed CJK Letters and Months
		/// </summary>
		/// <remarks>
		/// 不含中文，因为加了含中文的一个16pt大小字体的直接暴涨到28MB，再加几个 font-size 估计直接上天
		/// </remarks>
		public SpriteFont DebugFont { get; private set; } = null!;
		private readonly ServiceCollection _services = new();
		public FageCommonResources CommonResources { get; private set; } = null!;

		public FageScriptLoader ScriptLoader { get; private set; } = null!;

		private AudioManager _audioManager = null!;
		public AudioManager AudioManager => _audioManager;

		public bool SplashScreenDisabled { get; private set; } = false;

		public IConfiguration Configuration => configurationManager;
		public ServiceProvider DiProvider { get; private set; } = null!;

		public IReadOnlyList<string> GenericFontFamilyFileNames { get; set; } = null!;
		public string TitleBackgroundTextureName { get; private set; } = null!;
		public string FontSearchPath { get; private set; } = null!;
		public string DebugFontName { get; set; } = null!;
		public FontSystem GenericFontFamily { get; private set; } = null!;
		public Lua LuaVM { get; }

		public readonly TouchListener TouchListener = new();
		public readonly GamePadListener GamePadListener = new();

		#region 组件
		private Scene _currentScene = null!;
		private MainScene _mainScene = null!;
		private TitleScene _titleScene = null!;
		private SplashScreenScene? _splashScreenScene = null;
		#endregion


		public FageTemplateGame()
		{
			GraphicsManager = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = true;
			configurationManager = new ConfigurationManager();
			GraphicsManager.GraphicsProfile = GraphicsProfile.HiDef;
			GraphicsManager.PreferredBackBufferWidth = 1280;
			GraphicsManager.PreferredBackBufferHeight = 720;

			LuaVM = new Lua(LuaIntegerType.Int32, LuaFloatType.Float);

			AddConfigurationProviders(configurationManager);
			_services.AddSingleton(Configuration);
		}

		protected virtual void AddConfigurationProviders(IConfigurationManager configuration)
		{
			configuration.AddJsonFile("appsettings.json")
				.AddJsonFile("FageStartup.json")
				.AddCommandLine(Environment.GetCommandLineArgs());
		}

		protected virtual void ConfigureOptions(IServiceCollection services)
		{
			services.AddOptions<BranchPanelOptions>()
				.BindConfiguration("FageStartup:MainScene:BranchPanel",
				b => b.ErrorOnUnknownConfiguration = true)
				.Validate(opt =>
				{
					string basepath = Path.GetFullPath(Path.Combine(Content.RootDirectory, MainScene.ContentScopePath));

					ThrowIfAssetNotExist(Path.Combine(basepath, opt.OptionBackground), "缺少资源：选项按钮材质");
					ThrowIfAssetNotExist(Path.Combine(basepath, opt.OptionHoverBackground), "缺少资源：选项按钮高亮材质");
					ThrowIfAssetNotExist(Path.Combine(basepath, opt.OptionPressedBackground), "缺少资源：选项按钮选中材质");

					return true;
				});
		}

		protected static void ThrowIfAssetNotExist(string assetName, string message)
		{
			string assetPath = Path.ChangeExtension(assetName, ".xnb");
			if (!Path.Exists(assetPath))
				throw new MissingAssetException(message, assetPath);
		}

		protected virtual void ConfigureServices(IServiceCollection services)
		{
			
		}

		protected virtual void AddHostServices()
		{
			_services.AddSingleton(this);
			_services.AddSingleton<Game>(this);
			_services.AddSingleton(Services);
			_services.AddSingleton<IConfiguration>(configurationManager);
			_services.AddSingleton(GraphicsManager);
			_services.AddOptions();
		}

		protected override void Initialize()
		{
			AddHostServices();
			ConfigureOptions(_services);
			ConfigureServices(_services);

			DiProvider = _services.BuildServiceProvider(true);

			DebugFontName = Configuration["FageStartup:DebugFontName"] ?? "DefaultFont";
			FontSearchPath = Configuration["FageStartup:FontSearchPath"] ?? "Content";
			GenericFontFamilyFileNames = Configuration.GetSection("FageStartup:GenericFontFamilyFileNames").Get<IReadOnlyList<string>>()
				?? throw new ArgumentNullException(nameof(GenericFontFamilyFileNames), "FageStartup.json中，未配置常规字体");

			TitleBackgroundTextureName = Configuration["FageStartup:TitleScreen:TitleBackgroundTextureName"]
				?? throw new ArgumentNullException(nameof(TitleBackgroundTextureName), "必须配置标题界面的背景图");

			_spriteBatch = new(GraphicsDevice);
			_mainScene = new MainScene(this);
			_titleScene = new TitleScene(this);
			_audioManager = new AudioManager(this);
			ScriptLoader = new(this, _mainScene);

			base.Initialize();
		}

		protected override void LoadContent()
		{
#if MONOGAME_WINDOWSDX
			MatchDeviceRefreshRate();
#endif
			DebugFont = Content.Load<SpriteFont>(DebugFontName);
			GenericFontFamily = new FontSystem();

			foreach (var fontFaceName in GenericFontFamilyFileNames)
			{
				GenericFontFamily.AddFont(File.OpenRead(Path.Combine(FontSearchPath, fontFaceName)));
			}

			CommonResources = new(DebugFont, DiProvider, Services, Configuration, GenericFontFamily);

			_mainSceneInitializeTask = InitializeMainSceneBackgroundAsync();
			_titleSceneInitializeTask = InitializeTitleSceneBackgroundAsync();

			if (!(SplashScreenDisabled = Configuration.GetSection("SkipSplash").Get<bool>()))
			{
				_currentScene = _splashScreenScene = new(this);
				_splashScreenScene.SceneCompleted += SplashScreenCompleted;
				_splashScreenScene.LoadContent();
				Components.Add(_splashScreenScene);
			}
			else
			{
				SwitchToTitleScene();
			}

		}

		private async Task InitializeMainSceneBackgroundAsync()
			=> await Task.Run(_mainScene.LoadContent).ConfigureAwait(false);

		private Task InitializeTitleSceneBackgroundAsync()
		{
			return Task.Run(_titleScene.LoadContent);
		}

		private async ValueTask<FageScript> LoadMainSceneWithScript(string scriptName, CancellationToken ct)
		{
			var script = await ScriptLoader.LoadAsync(scriptName, ct).ConfigureAwait(false);
			await ScriptLoader.LoadResourceAsync(script, ct).ConfigureAwait(false);

			return script;
		}


		private async Task SwitchToSceneAsync(Scene targetScene, Task targetSceneInitializeTask,
			Func<CancellationToken, ValueTask> preLoad, Func<Scene, CancellationToken, ValueTask> load,
			CancellationToken ct)
		{
			Debug.Assert(targetScene != null);
			Debug.Assert(targetSceneInitializeTask != null);

			await targetSceneInitializeTask.ConfigureAwait(false);
			await preLoad(ct).ConfigureAwait(false);

			Scene deactivatingScene = _currentScene;
			Components.Remove(deactivatingScene);

			await load(targetScene, ct).ConfigureAwait(false);

			targetScene.Activate();
			Components.Add(targetScene);
			_currentScene = targetScene;


			deactivatingScene?.Deactivate();
		}

		private Task BeginSwitchToScene(Scene targetScene, Task targetSceneInitializeTask,
			Func<CancellationToken, ValueTask> preLoad, Func<Scene, CancellationToken, ValueTask> load,
			CancellationToken ct = default)
		{
			if (_switchingSceneTask != null && !_switchingSceneTask.IsCompleted)
				return Task.CompletedTask;


			using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

			var oldValue = _switchingSceneTask;
			var task = SwitchToSceneAsync(targetScene, targetSceneInitializeTask, preLoad, load, cts.Token);


			if (!ReferenceEquals(Interlocked.CompareExchange(ref _switchingSceneTask, task, oldValue), oldValue))
			{
				cts.Cancel();
				return Task.CompletedTask;
			}

			return task;
		}

		// 感觉这东西越来越抽象了，不放出去先
		internal void SwitchToMainScene(string scriptName, string? anchor = null)
		{
			_mainSceneInitializeTask ??= InitializeMainSceneBackgroundAsync();
			BeginSwitchToScene(_mainScene, _mainSceneInitializeTask, async (ct) =>
			{
				var loadedScript = await LoadMainSceneWithScript(scriptName, ct).ConfigureAwait(false);
				ct.ThrowIfCancellationRequested();
				_mainScene.Script = loadedScript;
			}, (ms, ct) =>
			{
				_mainScene.InternalCreateScriptingEnvironment();
				if (ct.IsCancellationRequested)
				{
					_mainScene.Deactivate();
					return ValueTask.FromCanceled(ct);
				}

				if (anchor != null)
				{
					// 好孩子不要学
					_mainScene.ScriptingEnvironment.JumpToAnchorThisScript(_mainScene.Script.Anchors[anchor]);
				}

				return ValueTask.CompletedTask;
			}).ContinueWith((t) =>
			{
				try
				{
					t.GetAwaiter().GetResult();
				}
				catch (NotSupportedException e)
				{
					AlertForCriticalError("脚本版本不在支持列表中", e);
				}
				catch (Exception e)
				{
					AlertForCriticalError("切换到主场景时发生错误", e);
				}
			});
		}

		internal void SwitchToTitleScene()
		{
			Debug.Assert(_titleSceneInitializeTask != null);
			// await _titleSceneInitializeTask.ConfigureAwait(false);
			BeginSwitchToScene(_titleScene, _titleSceneInitializeTask, (ct) => ValueTask.CompletedTask, (ts, ct) => ValueTask.CompletedTask);
		}

		private void SplashScreenCompleted(Scene splashScreen)
		{
			Components.Remove(_splashScreenScene);
			Task.Run(splashScreen.UnloadContent).ContinueWith(_ => _splashScreenScene = null!);

			if (SplashScreenDisabled)
				return;

			Debug.Assert(_titleSceneInitializeTask != null, "重复加载？");

			SwitchToTitleScene();
		}

		protected override void Update(GameTime gameTime)
		{
			if (_currentScene == null)
			{
				SuppressDraw();
				return;
			}

			KeyboardState keyboardState = Keyboard.GetState();

			TouchListener.Update(gameTime);
			GamePadListener.Update(gameTime);

			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
				Exit();

			if (keyboardState.IsKeyDown(Keys.Escape))
				Exit();

			// 左Alt+Enter切换全屏，和其它DX游戏类似
			if (keyboardState.IsKeyDown(Keys.Enter) && keyboardState.IsKeyDown(Keys.LeftAlt))
			{
				FageToggleFullscreen();
			}

			if (keyboardState.IsKeyUp(Keys.NumPad5))
			{
				_mainScene.TextArea.TextPresentingOptions.TextSpeed.Value = 0.9;
			}

			if (keyboardState.IsKeyDown(Keys.NumPad5))
			{
				_mainScene.TextArea.TextPresentingOptions.TextSpeed.Value = 0.5;
			}

			_audioManager.Update(gameTime);
			base.Update(gameTime);
		}

		private void FageToggleFullscreen()
		{
			int newHeight, newWidth;

			if (!GraphicsManager.IsFullScreen)
			{
				var displayMode = GraphicsDevice.Adapter.CurrentDisplayMode;
				GraphicsManager.PreferredBackBufferHeight = newHeight = displayMode.Height;
				GraphicsManager.PreferredBackBufferWidth = newWidth = displayMode.Width;
			}
			else
			{
				// TODO 获取之前储存的大小
				GraphicsManager.PreferredBackBufferWidth = newWidth = 1280;
				GraphicsManager.PreferredBackBufferHeight = newHeight = 720;
			}

			var newViewportParameters = GraphicsDevice.Viewport;
			newViewportParameters.Height = newHeight;
			newViewportParameters.Width = newWidth;
			GraphicsDevice.Viewport = newViewportParameters;

			GraphicsManager.ToggleFullScreen();
			GraphicsManager.ApplyChanges();
		}

		private bool ShouldPauseForMessageBox()
		{
			if (_criticalErrorInfo != null)
			{
				StringBuilder messageBuilder = new(30);
				ReadOnlySpan<char> span = _criticalErrorInfo.Item2.Message;

				const int LineLength = 40;

				while (span.Length >= LineLength)
				{
					messageBuilder.AppendLine($"{span[..LineLength]}");
					span = span[LineLength..];
				}

				messageBuilder.Append($"{span}");

				_alertMessageTask = MessageBox.Show(_criticalErrorInfo.Item1,
					messageBuilder.ToString(),
					["退出"]);

				_criticalErrorInfo = null;
			}

			if (_alertMessageTask != null)
			{
				SuppressDraw();
				if (_alertMessageTask.IsCompleted && !MessageBox.IsVisible)
				{
					Exit();
				}
				return true;
			}
			return false;
		}

		private void AlertForCriticalError(string messageBoxTitle, Exception e)
		{
			_criticalErrorInfo = new(messageBoxTitle, e);
		}

#if DEBUG
		private const string s_DebugHint = "You are using a debug build of FAGE.";
#endif
		protected override void Draw(GameTime gameTime)
		{
			if (ShouldPauseForMessageBox())
			{
				return;
			}

			GraphicsDevice.Clear(Color.CornflowerBlue);

			_spriteBatch.Begin();
#if DEBUG
			var debugHintMetrics = DebugFont.MeasureString(s_DebugHint);
			var screenWidth = GraphicsDevice.Viewport.Width;
			_spriteBatch.DrawString(DebugFont, s_DebugHint, new(Layout.AlignCenter(0, screenWidth, debugHintMetrics.X), 0), Color.White);
#endif
			_currentScene.Draw(gameTime, _spriteBatch);
			_spriteBatch.End();

			base.Draw(gameTime);
		}

		protected override void UnloadContent()
		{
			_mainScene.UnloadContent();
			_currentScene.UnloadContent();
			base.UnloadContent();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				LuaVM.Dispose();

				_audioManager.Dispose();
				_audioManager = null!;

				GenericFontFamily.Dispose();
				GenericFontFamily = null!;

				CommonResources.Dispose();
				CommonResources.DiServices.Dispose();

				configurationManager.Dispose();

				_spriteBatch.Dispose();
				_spriteBatch = null!;

				GraphicsManager.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}