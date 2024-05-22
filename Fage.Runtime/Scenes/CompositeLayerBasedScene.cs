using Fage.Runtime.Layering;
using MonoGame.Extended.Input.InputListeners;

namespace Fage.Runtime.Scenes;

public abstract class CompositeLayerBasedScene(string rootLayerName, FageTemplateGame game)
	: Scene(game)
{
	protected readonly SealedCompositeLayer RootLayer = new(rootLayerName);

	protected void UpdateRootLayer(GameTime gameTime)
	{
		var game = Game;
		RootLayer.DispatchMouseAsRoot(game);
		RootLayer.DispatchKeyboardAsRoot(game);
		RootLayer.Update(gameTime);
	}

	internal protected virtual void AcceptTouchInput(object? sender, TouchEventArgs mgxTouch)
	{
		RootLayer.DispatchTouchAsRoot(Game, mgxTouch);
	}

	internal protected virtual void AcceptGamePadInput(object? sender, GamePadEventArgs mgxGamePad)
	{
		RootLayer.DispatchGamePadAsRoot(Game, mgxGamePad);
	}

	protected override void OnScreenAwake()
	{
		base.OnScreenAwake();
		var touchListener = Game.TouchListener;
		touchListener.TouchStarted += AcceptTouchInput;
		touchListener.TouchEnded += AcceptTouchInput;
		touchListener.TouchMoved += AcceptTouchInput;
		touchListener.TouchCancelled += AcceptTouchInput;

		var gamepadListener = Game.GamePadListener;
		gamepadListener.ButtonDown += AcceptGamePadInput;
		gamepadListener.ButtonUp += AcceptGamePadInput;
		gamepadListener.ButtonRepeated += AcceptGamePadInput;
		gamepadListener.ThumbStickMoved += AcceptGamePadInput;
		gamepadListener.TriggerMoved += AcceptGamePadInput;
	}

	protected override void OnScreenSleep()
	{
		var touchListener = Game.TouchListener;
		touchListener.TouchStarted -= AcceptTouchInput;
		touchListener.TouchEnded -= AcceptTouchInput;
		touchListener.TouchMoved -= AcceptTouchInput;
		touchListener.TouchCancelled -= AcceptTouchInput;

		var gamepadListener = Game.GamePadListener;
		gamepadListener.ButtonDown -= AcceptGamePadInput;
		gamepadListener.ButtonUp -= AcceptGamePadInput;
		gamepadListener.ButtonRepeated -= AcceptGamePadInput;
		gamepadListener.ThumbStickMoved -= AcceptGamePadInput;
		gamepadListener.TriggerMoved -= AcceptGamePadInput;
		base.OnScreenSleep();
	}
}
