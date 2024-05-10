using System.Text.Json.Serialization;

namespace Fage.Runtime.Scenes.Splash;

public class SplashScreenItem(SplashScreenItemType type, string resourceName)
{
	[JsonConstructor]
	internal SplashScreenItem() : this(default, default!) { }

	public SplashScreenItemType Type { get; set; } = type;
	public string ResourceName { get; set; } = resourceName;
	public TimeSpan? Duration { get; internal set; }
}
