using Microsoft.Xna.Framework.Graphics;
using SharpDX;
using SharpDX.DXGI;

namespace Fage.Runtime;

file static class GraphicsInterop
{
	private readonly static Dictionary<SurfaceFormat, Format> FormatDictionary = new()
	{
		[SurfaceFormat.Color] = Format.R8G8B8A8_UNorm,
		[SurfaceFormat.Bgr565] = Format.B5G6R5_UNorm
	};

	internal static Format TranslateSurfaceFormatBack(SurfaceFormat surfaceFormat)
	{
		return FormatDictionary[surfaceFormat];
	}
}

public partial class FageTemplateGame
{
	public void MatchDeviceRefreshRate()
	{
		Factory2 factory = new();

		Adapter1 deviceUsing = factory.Adapters1.Single(a => a.Description1.DeviceId == GraphicsDevice.Adapter.DeviceId);
		Output monitorUsing = deviceUsing.Outputs.Single(o => o.Description.MonitorHandle == GraphicsDevice.Adapter.MonitorHandle);
		SurfaceFormat mgFormat = GraphicsDevice.DisplayMode.Format;

		var displayModes = monitorUsing.GetDisplayModeList(
			GraphicsInterop.TranslateSurfaceFormatBack(mgFormat),
			DisplayModeEnumerationFlags.Interlaced
		);

		// 不知道为什么，刷新率的分子分母是反过来的
		Array.Sort(displayModes, (l, r) =>
		{
			Rational rl = l.RefreshRate, rr = r.RefreshRate;
			return (rl.Denominator * rr.Numerator) - (rl.Numerator * rr.Denominator);
		});
		
		DisplayMode currentDisplayMode = GraphicsDevice.Adapter.CurrentDisplayMode;
		var sameAspectRatioModes = displayModes.Where(dm =>
			(float)dm.Width / dm.Height
				== currentDisplayMode.AspectRatio
		);

		var fallbackDisplayMode = sameAspectRatioModes.First();

		var selectedDisplayMode = sameAspectRatioModes.FirstOrDefault(dm => dm.Height == currentDisplayMode.Height
				&& dm.Width == currentDisplayMode.Width, fallbackDisplayMode);

		// 分子分母同样是反的
		TargetElapsedTime = TimeSpan.FromTicks(
			TimeSpan.TicksPerSecond * selectedDisplayMode.RefreshRate.Denominator
				/ selectedDisplayMode.RefreshRate.Numerator
		);
	}
}
