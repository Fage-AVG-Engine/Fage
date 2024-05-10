using Fage.Runtime.Utility;
using Microsoft.Extensions.Configuration;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;

namespace Fage.Runtime.Scenes.Splash;

public class SplashScreenScene : Scene
{
	public const string SplashOptionsConfigKey = "SplashOptions";
	public const string SplashScreenItemsConfigKey = "SplashScreenItems";

	private VideoPlayer _player = null!;
	private Rectangle _sourceRect = default;

	private readonly SplashOptions _options = new();

	private TimeSpan _contentPlayed = new(0);
	private readonly Curve _contentAlphaCurve;

	private readonly SplashResource[] _splashResources = null!;
	private int _currentSplashIndex = -1;
	private ParallelLoopResult _loadResourceParallellyResult;

	private bool InSplash => _currentSplashIndex > -1 && _currentSplashIndex < _splashResources.Length;
	private SplashResource CurrentSplashScreen => _splashResources[_currentSplashIndex];

	private ContentManager GameContent => Game.Content;

	public SplashScreenScene(FageTemplateGame game) : base(game)
	{
		var options = game.Configuration.GetSection("FageStartup:SplashScreen");
		options.GetSection(SplashOptionsConfigKey).Bind(_options);
		_options.Reload();

		var splashItems = options.GetSection(SplashScreenItemsConfigKey).Get<SplashScreenItem[]>() ?? [];
		_splashResources = splashItems.Select(i => new SplashResource(i)).ToArray();

		_contentAlphaCurve = new()
		{
			PreLoop = CurveLoopType.Constant,
			PostLoop = CurveLoopType.Constant,
		};
	}

	private void SetAlphaCurveByFading(SplashOptions fading, TimeSpan duration)
	{
		var keys = _contentAlphaCurve.Keys;
		double p1 = fading.FadeInTime.TotalMilliseconds;
		double p2 = (duration - fading.FadeOutTime).TotalMilliseconds;
		double pEnd = duration.TotalMilliseconds;
		keys.Clear(); //[1].Position = fading.FadeInTime.Milliseconds
		keys.Add(new(0, 0) { Continuity = CurveContinuity.Smooth });
		keys.Add(new((float)p1, 1) { Continuity = CurveContinuity.Smooth });
		keys.Add(new((float)p2, 1) { Continuity = CurveContinuity.Smooth });
		keys.Add(new((float)pEnd, 0) { Continuity = CurveContinuity.Smooth });
	}

	public override void LoadContent()
	{
		_player = new();
		_loadResourceParallellyResult = Parallel.ForEach(
			_splashResources,
			res => res.LoadResource(GameContent, _options.DefaultImageDurationSeconds)
		);

		if (_splashResources.Length != 0)
		{
			SpinWait waiter = new();
			while (!_splashResources[0].Loaded)
				waiter.SpinOnce();
		}
	}

	public override void UnloadContent()
	{
		foreach (var res in _splashResources)
		{
			GameContent.UnloadAsset(res.ItemInfo.ResourceName);
		}
	}
	protected override void SetupInitialContent()
	{
		if (Game.SplashScreenDisabled)
		{
			_currentSplashIndex = _splashResources.Length;
			Task.Run(UnloadContent);
			Game.SuppressDraw();

			TriggerSceneCompleted(this);
			return;
		}

		NextItem();
	}

	public override void Update(GameTime gameTime)
	{
		SetupInitialContentIfNecessary();

		if (Game.SplashScreenDisabled)
			return;

		if (SplashItemCompleted(gameTime)
			&& _currentSplashIndex < _splashResources.Length - 1) // 当前项目播放完成，而且后面还有项目
		{
			NextItem();
		}
		else if (_currentSplashIndex == _splashResources.Length - 1
			&& SplashItemCompleted(gameTime)) // 全部放完，触发时间
		{
			TriggerSceneCompleted(this);
		}

		_contentPlayed += gameTime.ElapsedGameTime;

		base.Update(gameTime);

		bool SplashItemCompleted(GameTime gameTime)
		{
			return _contentPlayed + gameTime.ElapsedGameTime > CurrentSplashScreen.Duration + _options.InterludeTime;
		}
	}

	private void NextItem()
	{
		_currentSplashIndex++;

		switch (CurrentSplashScreen.Type) // 计算 source rect
		{
			case SplashScreenItemType.Video:
				int width = CurrentSplashScreen.VideoResource!.Width;
				int height = CurrentSplashScreen.VideoResource!.Height;
				_sourceRect = AdaptBounds.FillDestinationByCenter(
					new Rectangle(0, 0, width, height),
					GraphicsDevice.Viewport.Bounds
				);
				break;
			case SplashScreenItemType.Image:
				_sourceRect = AdaptBounds.FillDestinationByCenter(
					CurrentSplashScreen.ImageResource!.Bounds,
					GraphicsDevice.Viewport.Bounds
				);
				// 设置 alpha 曲线
				SetAlphaCurveByFading(_options, CurrentSplashScreen.Duration);
				break;
			default:
				break;
		}


		// 开始播放内容并计时
		_contentPlayed = new(0);
		if (CurrentSplashScreen.IsVideo)
			_player.Play(CurrentSplashScreen.VideoResource);
	}

	public override void Draw(GameTime gameTime, SpriteBatch batch)
	{
		if (Game.SplashScreenDisabled) return;

		batch.End();
		// 不使用外部提供的SpriteBatch。默认设置下，颜色的 alpha 分量 mask无效
		batch.Begin(blendState: BlendState.NonPremultiplied);
		batch.GraphicsDevice.Clear(Color.Black);
		if (CurrentSplashScreen.Loaded)
		{
			var currentSplashScreen = CurrentSplashScreen;
			switch (currentSplashScreen.Type)
			{
				case SplashScreenItemType.Video:
					batch.Draw(_player.GetTexture(),
						GraphicsDevice.Viewport.Bounds,
						_sourceRect,
						Color.White);
					break;
				case SplashScreenItemType.Image:
					Color colorMask = new(
						Color.White,
						_contentAlphaCurve.Evaluate((float)_contentPlayed.TotalMilliseconds)
					);
					batch.Draw(currentSplashScreen.ImageResource,
						GraphicsDevice.Viewport.Bounds,
						_sourceRect,
						colorMask);
					break;
				default:
					Debug.WriteLine($"类型为{currentSplashScreen.Type}的" +
						$"开屏动画（序号{_currentSplashIndex}，路径{currentSplashScreen.ItemInfo.ResourceName}）" +
						$"不受支持，请检查配置文件。");
					NextItem();
					break;
			}
		}
	}
}
