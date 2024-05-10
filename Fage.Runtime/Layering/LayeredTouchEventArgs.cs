using MonoGame.Extended.Input.InputListeners;

namespace Fage.Runtime.Layering;

public class LayeredTouchEventArgs(TouchEventArgs touch, Game game) : LayeredInputEventArgs(game)
{
	public TouchEventArgs Touch { get; } = touch;
}
