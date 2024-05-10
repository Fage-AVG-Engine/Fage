using MonoGame.Extended.Input;

namespace Fage.Runtime.Layering;

public class LayeredKeyboardEventArgs(Game game) : LayeredInputEventArgs(game)
{
	public readonly KeyboardStateExtended State = KeyboardExtended.GetState();
}
