using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace Fage.Runtime.Utility;

public static class Layout
{
	public static Rectangle[] VerticalStackJustifyStart(Rectangle destinationArea, IReadOnlyList<Point> itemSizes, int itemGap)
	{
		int itemsCount = itemSizes.Count;
		Rectangle[] result = new Rectangle[itemsCount];
		int x = destinationArea.Left, y = destinationArea.Top;

		int verticalSpacing = itemGap;
		for (int i = 0; i < itemsCount; i++)
		{
			Point currentItemSize = itemSizes[i];
			result[i] = new(x, y, currentItemSize.X, currentItemSize.Y);
			y += currentItemSize.Y + verticalSpacing;
		}

		return result;
	}

	public static Rectangle[] VerticalStackSpaceAround(Rectangle destinationArea, IReadOnlyList<Point> itemSizes, int paddingStart = 0, int paddingEnd = 0)
	{
		int itemsCount = itemSizes.Count;
		Rectangle[] result = new Rectangle[itemsCount];
		int x = destinationArea.Left, y = destinationArea.Top;

		int verticalAvailableSpace = destinationArea.Bottom;

		int firstItemHalfHeight = itemSizes[0].Y / 2;
		int lastItemHalfHeight = itemSizes[itemSizes.Count - 1].Y / 2;

		y += paddingStart + firstItemHalfHeight;
		verticalAvailableSpace -= firstItemHalfHeight + lastItemHalfHeight + paddingEnd;

		int verticalSpacing = verticalAvailableSpace / (itemsCount - 1);

		for (int i = 0; i < itemsCount; i++)
		{
			Point currentItemSize = itemSizes[i];
			result[i] = new(x, y - currentItemSize.Y / 2, currentItemSize.X, currentItemSize.Y);
			y += verticalSpacing;
		}

		return result;
	}

	public static Rectangle[] VerticalStackJustifyEvenly(Rectangle destinationArea, IReadOnlyList<Point> itemSizes, int paddingStart = 0, int paddingEnd = 0)
	{
		int itemsCount = itemSizes.Count;
		Rectangle[] result = new Rectangle[itemsCount];
		int x = destinationArea.Left, y = destinationArea.Top;

		int verticalAvailableSpace = destinationArea.Height;

		y += paddingStart;
		verticalAvailableSpace -= paddingEnd;

		int axesVerticalSpacing = verticalAvailableSpace / (itemsCount + 1);

		for (int i = 0; i < itemsCount; i++)
		{
			Point currentItemSize = itemSizes[i];
			y += axesVerticalSpacing;
			result[i] = new(x, y - currentItemSize.Y / 2, currentItemSize.X, currentItemSize.Y);
		}

		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int AlignCenter(int startLocation, int containerDimension, int itemDimension)
		=> startLocation + ((containerDimension - itemDimension) / 2);

	public static void HorizontalAlignCenter(ref Rectangle itemBoundingBox, int containerWidth)
		=> itemBoundingBox.X = AlignCenter(itemBoundingBox.X, containerWidth, itemBoundingBox.Width);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float AlignCenter(float startLocation, float containerDimension, float itemDimension)
		=> startLocation + ((containerDimension - itemDimension) / 2);

	/// <summary>
	/// 将指定大小的矩形放置在<paramref name="destinationArea"/>中心，结果通过<paramref name="destinationArea"/>传出
	/// </summary>
	/// <param name="destinationArea"></param>
	/// <param name="sourceWidth"></param>
	/// <param name="sourceHeight"></param>
	public static void RectangleAlignCenter(ref Rectangle destinationArea, int sourceWidth, int sourceHeight)
	{
		destinationArea.X = AlignCenter(destinationArea.X, destinationArea.Width, sourceWidth);
		destinationArea.Y = AlignCenter(destinationArea.Y, destinationArea.Height, sourceHeight);
		destinationArea.Width = sourceWidth;
		destinationArea.Height = sourceHeight;
	}
}
