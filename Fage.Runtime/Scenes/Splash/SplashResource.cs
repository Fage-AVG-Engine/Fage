using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace Fage.Runtime.Scenes.Splash;

internal class SplashResource(SplashScreenItem itemInfo)
{
	public SplashScreenItem ItemInfo { get; set; } = itemInfo;

	[MemberNotNullWhen(true, nameof(VideoResource))]
	public bool IsVideo => Type == SplashScreenItemType.Video;

	[MemberNotNullWhen(true, nameof(ImageResource))]
	public bool IsImage => Type == SplashScreenItemType.Image;

	public Video? VideoResource { get; private set; }

	public Texture2D? ImageResource { get; private set; }

	public SplashScreenItemType Type => ItemInfo.Type;

	public TimeSpan Duration
	{
		get
		{
			if (IsVideo)
			{
				return VideoResource.Duration;
			}
			else
			{
				Debug.Assert(ItemInfo.Duration.HasValue);
				return ItemInfo.Duration.GetValueOrDefault();
			}
		}
	}

	public bool Loaded { get; private set; }

	public void LoadResource(ContentManager content, float defaultImageDurationSeconds)
	{
		switch (Type)
		{
			case SplashScreenItemType.Video:
				VideoResource = content.Load<Video>(ItemInfo.ResourceName);
				break;
			case SplashScreenItemType.Image:
				ImageResource = content.Load<Texture2D>(ItemInfo.ResourceName);
				if (!ItemInfo.Duration.HasValue)
					ItemInfo.Duration = TimeSpan.FromSeconds(defaultImageDurationSeconds);
				break;
			default:
				throw new InvalidOperationException($"不支持{Type}类型的启动屏幕项。");
		}
		Loaded = true;
	}
}
