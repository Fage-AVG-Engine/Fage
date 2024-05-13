using Microsoft.Xna.Framework.Graphics;

namespace Fage.Runtime.Scenes;

public abstract class Scene(FageTemplateGame game) : GameComponent(game)
{
	private bool _initialContentSet;

	public new FageTemplateGame Game => (FageTemplateGame)base.Game;

	public GraphicsDevice GraphicsDevice => Game.GraphicsDevice;

	public FageCommonResources CommonResources => Game.CommonResources;

	public FageServiceScope RootServiceProvider => CommonResources.Services;

	public event Action<Scene>? SceneCompleted;

	public readonly List<Action<GameTime>> AdditionalUpdatable = [];

	protected IServiceProvider DiServices => Game.DiProvider;

	protected void TriggerSceneCompleted(Scene scene)
	{
		SceneCompleted?.Invoke(scene);
	}

	public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);
	public abstract void LoadContent();
	public abstract void UnloadContent();
	protected virtual void SetupInitialContent() { }

	protected void SetupInitialContentIfNecessary()
	{
		if (!_initialContentSet)
		{
			_initialContentSet = true;
			SetupInitialContent();
		}
	}

	protected virtual void OnScreenAwake() { }

	protected virtual void OnScreenSleep() { }

	internal void Activate()
	{
		OnScreenAwake();
	}

	internal void Deactivate() 
	{
		OnScreenSleep();
	}
}
