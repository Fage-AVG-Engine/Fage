namespace Fage.Runtime.Layering
{
	public interface ILayeredInputHandler<T> where T: LayeredInputEventArgs
	{
		public bool HandleInput(ILayer sender, T e);
	}
}