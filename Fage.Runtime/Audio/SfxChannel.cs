﻿using Microsoft.Xna.Framework.Audio;

namespace Fage.Runtime.Audio;

public class SfxChannel : AudioChannelBase, IDisposable
{
	private const int SafeUnloadDelay = 10;
	// 不要传出这个类以外
	private readonly List<(SoundEffect, TimeSpan SuggestedUnloadTime)> _unloadQueue;
	private bool disposedValue;
	private bool _unloading;

	internal SfxChannel(AudioManager audioManager)
		: base(audioManager)
	{
		Volume = audioManager.SfxVolume;
		_unloadQueue = new(8);
	}

	protected internal override void Update(GameTime gameTime)
	{

	}

	public void PlaySfx(string effectName)
	{
		if (!_unloading && !disposedValue)
		{
			var effect = Contents.Load<SoundEffect>(effectName);
			effect.Play(Volume.Value, 0, 0);
			_unloadQueue.Add((effect, EstimatedAudioThreadTime + effect.Duration + TimeSpan.FromMilliseconds(SafeUnloadDelay)));
		}
		else
		{
			throw new ObjectDisposedException(nameof(SfxChannel), "音效通道已释放，不能播放新的音效");
		}
	}

	public void UnloadPlayedEffects()
	{
		for (int i = _unloadQueue.Count - 1; i != 0; i--)
		{
			var effect = _unloadQueue[i];
			if (effect.SuggestedUnloadTime < EstimatedAudioThreadTime)
			{
				Contents.UnloadAsset(effect.Item1.Name);
				_unloadQueue.RemoveAt(i);
				i++;
			}
		}
	}

	public void UnloadResources()
	{
		_unloading = true;
		foreach (var effect in _unloadQueue)
		{
			Contents.UnloadAsset(effect.Item1.Name);
		}
		_unloadQueue.Clear();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				UnloadResources();
			}

			// 释放未托管的资源(未托管的对象)并重写终结器
			// 将大型字段设置为 null
			disposedValue = true;
		}
	}

	public void Dispose()
	{
		// 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
