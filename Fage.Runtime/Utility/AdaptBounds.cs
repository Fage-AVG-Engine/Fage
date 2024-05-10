namespace Fage.Runtime.Utility;

/// <summary>
/// 图像适应算法
/// </summary>
public static class AdaptBounds
{
	/// <summary>
	/// 保证不出现黑边、不拉伸原图的前提下，用中心部分填充到目标区域
	/// </summary>
	/// <param name="width">原图宽度</param>
	/// <param name="height">原图高度</param>
	/// <param name="destinationWidth">目标区域宽度</param>
	/// <param name="destinationHeight">目标区域高度</param>
	/// <param name="adaptedSourceBounds"></param>
	public static void FillDestinationByCenter(int width, int height, int destinationWidth, int destinationHeight, out Rectangle adaptedSourceBounds)
	{
		float arS = (float)width / height; // aspect ratio of Source
		float arD = (float)destinationWidth / destinationHeight; // aspect ratio of Destination

		// 计算得到的像素向下取整
		if (arS >= arD)
		{
			float widthOfSourceBound = height * arD;
			int xOfBound = (int)((width - widthOfSourceBound) / 2);

			adaptedSourceBounds = new(xOfBound, 0, (int)widthOfSourceBound, height);
		}
		else
		{
			float heightOfSourceBound = width / arD;
			int yOfBound = (int)((height - heightOfSourceBound) / 2);

			adaptedSourceBounds = new(0, yOfBound, width, (int)heightOfSourceBound);
		}
	}

	/// <summary>
	/// 保证不出现黑边、不拉伸原图的前提下，用中心部分填充到目标区域
	/// </summary>
	public static void FillDestinationByCenter(ref readonly Rectangle sourceBounds,
		ref readonly Rectangle destinationBounds,
		out Rectangle adaptedSourceBounds)
	{
		float arS = (float)sourceBounds.Width / sourceBounds.Height; // aspect ratio of Source
		float arD = (float)destinationBounds.Width / destinationBounds.Height; // aspect ratio of Destination

		// 计算得到的像素向下取整
		if (arS >= arD)
		{
			float widthOfSourceBound = sourceBounds.Height * arD;
			int xOfBound = (int)((sourceBounds.Width - widthOfSourceBound) / 2) + sourceBounds.X;

			adaptedSourceBounds = new(xOfBound, sourceBounds.Y, (int)widthOfSourceBound, sourceBounds.Height);
		}
		else
		{
			float heightOfSourceBound = sourceBounds.Width / arD;
			int yOfBound = (int)((sourceBounds.Height - heightOfSourceBound) / 2) + sourceBounds.Y;

			adaptedSourceBounds = new(sourceBounds.X, yOfBound, sourceBounds.Width, (int)heightOfSourceBound);
		}

	}

	/// <summary>
	/// 保证不出现黑边、不拉伸原图的前提下，用中心部分填充到目标区域
	/// </summary>
	public static Rectangle FillDestinationByCenter(Rectangle sourceBounds, Rectangle destinationBounds)
	{
		FillDestinationByCenter(in sourceBounds, in destinationBounds, out Rectangle result);
		return result;
	}

	/// <summary>
	/// 将原图按原比例适应到目标区域的中心
	/// </summary>
	/// <param name="sourceBounds"></param>
	/// <param name="destinationBounds"></param>
	/// <returns></returns>
	public static Rectangle FitDestinationByCenter(Rectangle sourceBounds, Rectangle destinationBounds)
	{
		FitDestinationByCenter(ref sourceBounds, ref destinationBounds, out var result);
		return result;
	}

	/// <summary>
	/// 将原图按原比例适应到目标区域的中心
	/// </summary>
	public static void FitDestinationByCenter(ref readonly Rectangle sourceBounds,
		ref readonly Rectangle destinationBounds,
		out Rectangle adaptedDestinationBounds)
	{
		FillDestinationByCenter(in destinationBounds, in sourceBounds, out adaptedDestinationBounds);
	}
}