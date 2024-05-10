using System.Runtime.InteropServices;

namespace Fage.Runtime.Utility;

public static class CheckDotnetVersion
{
	public static readonly bool RunningOnCore3AndLater = CheckRunningOnCore3Later();

	private static bool CheckRunningOnCore3Later()
	{
		var frameworkDesc = RuntimeInformation.FrameworkDescription;

		// .NET Framework x.x.x
		if (frameworkDesc.StartsWith(".NET Framework"))
		{
			return false;
		}

		// .NET Core x.x.x
		if (frameworkDesc.StartsWith(".NET Core "))
		{
			int majorNumberEnd = frameworkDesc.IndexOf('.', 10);
			return int.Parse(frameworkDesc.AsSpan().Slice(11, majorNumberEnd)) >= 3;
		}

		// .NET x.x.x
		if (frameworkDesc.StartsWith(".NET ") && frameworkDesc.LastIndexOf(' ') == 5)
		{
			return true;
		}

		// Old Mono?
		return false;
	}
}
