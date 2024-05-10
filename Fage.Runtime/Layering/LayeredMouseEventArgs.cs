using MonoGame.Extended.Input;

namespace Fage.Runtime.Layering;

public class LayeredMouseEventArgs(Game game) : LayeredInputEventArgs(game)
{
	public bool IsUserInGame
	{
		get
		{
			if (!IsGameActive)
				return false;

			// State.Position Game.Window.ClientBounds.Size;
			if (State.X < 0 && State.Y < 0)
				return false;

			var bounds = Game.Window.ClientBounds;

			return State.Y <= bounds.Height
				&& State.X <= bounds.Width;
		}
	}

	public readonly MouseStateExtended State = MouseExtended.GetState();
}
