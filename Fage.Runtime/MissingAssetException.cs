namespace Fage.Runtime;

[Serializable]
public class MissingAssetException : FileNotFoundException
{
	public MissingAssetException() { }
	public MissingAssetException(string message, string fileName) : base(message, fileName) { }
	public MissingAssetException(string message, Exception inner) : base(message, inner) { }
}