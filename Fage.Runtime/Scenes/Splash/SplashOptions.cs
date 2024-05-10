using System.Text.Json.Serialization;

namespace Fage.Runtime.Scenes.Splash;

public class SplashOptions
{
	public float FadeOutSeconds { get; set; } = 1f;
	public float FadeInSeconds { get; set; } = 1f;

	public float InterludeSeconds { get; set; } = 0.4f;

	public float DefaultImageDurationSeconds { get; set; } = 3f;

	[JsonIgnore]
	internal TimeSpan FadeOutTime { get; private set; }
	[JsonIgnore]
	internal TimeSpan FadeInTime { get; private set; }
	[JsonIgnore]
	internal TimeSpan InterludeTime { get; private set; }
	[JsonIgnore]
	internal TimeSpan ImageDurationTime { get; private set; }

	internal void Reload()
	{
		FadeOutTime = TimeSpan.FromSeconds(FadeOutSeconds);
		FadeInTime = TimeSpan.FromSeconds(FadeInSeconds);
		InterludeTime = TimeSpan.FromSeconds(InterludeSeconds);
		ImageDurationTime = TimeSpan.FromSeconds(DefaultImageDurationSeconds);
	}
}
