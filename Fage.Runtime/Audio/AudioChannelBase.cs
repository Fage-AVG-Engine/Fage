using Fage.Runtime.Bindable;
using Microsoft.Xna.Framework.Content;

namespace Fage.Runtime.Audio;

public abstract class AudioChannelBase
{
	protected AudioChannelBase(AudioManager audioManager)
	{
		Contents = audioManager.Contents;
		Volume = null!;
	}

	/// <remarks>
	/// 不要对它调用Unload或Dispose，它的生命周期由AudioManager管理
	/// </remarks>
	protected ContentManager Contents { get; }
	public BindableFloat Volume { get; protected init; }

	public TimeSpan EstimatedAudioThreadTime => new(Environment.TickCount64);

	protected internal abstract void Update(GameTime gameTime);
}