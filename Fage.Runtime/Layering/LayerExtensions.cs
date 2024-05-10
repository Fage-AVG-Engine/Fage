using System.Text;

namespace Fage.Runtime.Layering;

public static class LayerExtensions
{
	public static string GetLayerPath(this ILayer layer)
	{
		StringBuilder sb = new(layer.Name.Length);
		Stack<string> pathReversed = new();

		ILayer? currentNode = layer;

		do
		{
			pathReversed.Push(currentNode.Name);
			currentNode = currentNode.Parent;
		} while (currentNode != null);

		foreach (var pathComponent in pathReversed)
		{
			sb.Append('/').Append(pathComponent);
		}

		return sb.ToString();
	}
}
