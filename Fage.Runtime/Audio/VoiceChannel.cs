using Microsoft.Xna.Framework.Audio;
using System.Diagnostics;
using System.Threading.Channels;

namespace Fage.Runtime.Audio;

public class VoiceChannel : AudioChannelBase, IDisposable
{
	private SoundEffect? _currentVoiceSound;

	private readonly Channel<Tuple<SoundEffect, DateTime/* 预估的开始播放的时间点 */>> _resourcesToUnload;
	private readonly ChannelWriter<Tuple<SoundEffect, DateTime>> _unloadChannelWriter;
	private readonly Task _unloadResourceTask;

	/// <summary>
	/// 当前音频持续的时长
	/// </summary>
	private TimeSpan _voiceLasted;
	private bool disposedValue;

	internal VoiceChannel(AudioManager audioManager) : base(audioManager)
	{
		Volume = audioManager.VoiceVolume;
		_resourcesToUnload = Channel.CreateUnbounded<Tuple<SoundEffect, DateTime>>(new()
		{
			SingleReader = true,
			SingleWriter = true,
		});
		_unloadResourceTask = StartResourceUnloader();
		_unloadChannelWriter = _resourcesToUnload.Writer;
	}

	/// <summary>
	/// 给两段交叉的语音施加的间隔
	/// </summary>
	/// <remarks>
	/// <para>要播放的下一条语音，打断了正在播放的语音时，会给两条语音施加过渡效果。</para>
	/// <para>本属性控制过渡效果的时长。</para>
	/// </remarks>
	public TimeSpan VoiceInterval { get; set; }

	/// <summary>
	/// 切换语音时的防抖间隔
	/// </summary>
	/// <remarks>
	///	<para>为了在快进（甚至是玩家操作速度太快）时保持语音的连贯，当两次切换语音的时长小于防抖间隔时，将不会打断正在播放的语音，并推迟切换语音的时机。</para>
	///	<para>本属性控制防抖间隔的长度</para>
	/// </remarks>
	public TimeSpan DebounceTimeout { get; set; } = TimeSpan.FromSeconds(0.3);

	/// <summary>
	/// 指示异步卸载资源是否完成的<see cref="Task"/>
	/// </summary>
	internal Task ResourceUnloadingTask => _unloadResourceTask;

	/// <summary>
	/// 播放下一条语音
	/// </summary>
	/// <param name="voicePath">语音相对于{资源根目录}/Audio的路径。不会根据当前设置的角色进行解析。</param>
	public void NextVoice(string voicePath)
	{
		StartNextWithoutFading(voicePath);
	}

	/// <summary>
	/// 播放下一条语音，并且打断当前语音，不使用过渡效果
	/// </summary>
	/// <param name="voicePath"></param>
	public void StartNextWithoutFading(string voicePath)
	{
		if (CheckShouldNextVoiceStart())
		{
			// 语音播放时长大于防抖时长
			// 可以切换到下一条语音
			if (_currentVoiceSound != null)
				RequestUnloadSound();

			_currentVoiceSound = Contents.Load<SoundEffect>(voicePath);
			_currentVoiceSound.Play(Volume.Value, 0, 0);
		}
		else
		{
			// 处于快进状态，或用户操作速度太快
			// 重置播放时长，将它的值设为0，直到下次切换语音的间隔足够长，才实际切换到另一条语音
			_voiceLasted = new();
		}
	}

	private bool CheckShouldNextVoiceStart()
	{
		if (_currentVoiceSound == null)
			return true;

		return _voiceLasted > DebounceTimeout
			|| _currentVoiceSound.Duration < _voiceLasted + TimeSpan.FromMilliseconds(100);
	}

	protected internal sealed override void Update(GameTime gameTime)
	{
		if (_voiceLasted < _currentVoiceSound?.Duration)
		{
			_voiceLasted += gameTime.ElapsedGameTime;
		}
		else if (_currentVoiceSound != null)
		{
			RequestUnloadSound();
		}
	}

	/// <devdoc>
	/// <summary>
	/// 发出卸载<see cref="_currentVoiceSound"/>引用的语音资源的请求。
	/// </summary>
	/// <remarks>
	/// 调用方负责检查<see cref="_currentVoiceSound"/>是否为<see langword="null"/>。
	/// </remarks>
	/// </devdoc>
	private void RequestUnloadSound()
	{
		Debug.Assert(_currentVoiceSound != null);
		_unloadChannelWriter.TryWrite(new(_currentVoiceSound, DateTime.UtcNow - _voiceLasted));
		_currentVoiceSound = null;
		_voiceLasted = new();
	}

	private async Task StartResourceUnloader()
	{
		ChannelReader<Tuple<SoundEffect, DateTime>> reader = _resourcesToUnload.Reader;

		await foreach (var resource in reader.ReadAllAsync().ConfigureAwait(false))
		{
			DateTime utcNow = DateTime.UtcNow;
			DateTime estimatedEndTime = resource.Item2 + resource.Item1.Duration;
			while (estimatedEndTime < utcNow)
				// 避免轮询频率过高占用大量CPU
				await Task.Delay((int)((estimatedEndTime - utcNow).Ticks / TimeSpan.TicksPerMillisecond) + 5);

			Contents.UnloadAsset(resource.Item1.Name);
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				UnloadResources();
			}

			disposedValue = true;
		}
	}

	internal protected void UnloadResources()
	{
		if (_currentVoiceSound != null)
			RequestUnloadSound();

		_unloadChannelWriter.Complete();
	}

	public void Dispose()
	{
		// 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}