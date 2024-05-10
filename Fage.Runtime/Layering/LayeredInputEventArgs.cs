namespace Fage.Runtime.Layering;

public class LayeredInputEventArgs(Game game) : EventArgs
{
	public Game Game { get; } = game;

	public bool IsGameActive { get; } = game.IsActive;

	// public bool StopPropagation { get; set; } = false;

	// public void InputHandled() => StopPropagation = true;
}
