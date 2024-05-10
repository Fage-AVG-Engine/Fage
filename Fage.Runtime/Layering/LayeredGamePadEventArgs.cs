using MonoGame.Extended.Input.InputListeners;

namespace Fage.Runtime.Layering;

public class LayeredGamepadEventArgs(GamePadEventArgs gamepad, Game game) : LayeredInputEventArgs(game)
{
	public GamePadEventArgs GamePad { get; } = gamepad;
}
