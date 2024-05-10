using Microsoft.Xna.Framework.Media;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Fage.Runtime.Audio;

public class BackgroundMusicChannel : AudioChannelBase
{
	private Song? _currentBgmSong;
	private string? _currentBgm;

	private Song? _nextBgm;
	private string? _nextBgmName;

	internal BackgroundMusicChannel(AudioManager audioManager) : base(audioManager)
	{
		Volume = audioManager.BgmVolume;
	}

	internal Song? CurrentBgmSong => _currentBgmSong;

	public string? CurrentBgm
	{
		get => CurrentBgm;
		set
		{
			if (_currentBgm == value) return;
			SetNextBgm(value);
		}
	}
	/// <summary>
	/// 切换音乐时，音量的变化曲线
	/// </summary>
	/// <remarks>
	/// 曲线的开始位置是0，结束位置是切歌效果完成所经历的毫秒数，即<c>(SwitchingEndTime - SwitchingBeginTime).Milliseconds</c>
	/// </remarks>
	public Curve SwitchingVolumeCurve { get; set; } = new()
	{
		PreLoop = CurveLoopType.Constant,
		PostLoop = CurveLoopType.Constant,
	};

	/// <summary>
	/// 切歌过渡效果开始的时机
	/// </summary>
	public DateTime SwitchingBeginTime { get; set; }
	/// <summary>
	/// 切歌过渡效果结束的时机
	/// </summary>
	public DateTime SwitchingEndTime { get; set; }
	/// <summary>
	/// 处于切歌过渡效果中时，播放下一首音乐的时机
	/// </summary>
	public DateTime SwitchingNextBgmBeginTime { get; set; }

	internal void SetNextBgm(string? nextBgmName, Song nextBgm, TimeSpan fadeoutLength, TimeSpan fadeInLength, TimeSpan leadingSilenceLength)
	{
		TimeSpan switchingEffectDuration = fadeoutLength + leadingSilenceLength + fadeInLength;
		if (switchingEffectDuration == TimeSpan.Zero)
		{
			MediaPlayer.Play(nextBgm);
			return;
		}

		var curveKeys = SwitchingVolumeCurve.Keys;
		curveKeys.Clear();

		curveKeys.Add(new(0, 1f) { Continuity = CurveContinuity.Smooth });
		curveKeys.Add(new((float)fadeoutLength.TotalMilliseconds, 0f) { Continuity = CurveContinuity.Smooth });
		curveKeys.Add(new((float)(switchingEffectDuration - fadeInLength).TotalMilliseconds, 0f) { Continuity = CurveContinuity.Smooth });
		curveKeys.Add(new((float)switchingEffectDuration.TotalMilliseconds, 1f) { Continuity = CurveContinuity.Smooth });
		var now = DateTime.Now;
		SetNextBgm(
			nextBgmName,
			nextBgm,
			SwitchingVolumeCurve,
			now,
			now + fadeoutLength + leadingSilenceLength,
			now + switchingEffectDuration
		);
	}

	public void SetNextBgm(string nextBgmName, TimeSpan fadeoutLength, TimeSpan fadeInLength, TimeSpan leadingSilenceLength)
		=> SetNextBgm(nextBgmName, Contents.Load<Song>(Path.Combine("Bgm", nextBgmName)), fadeoutLength, fadeInLength, leadingSilenceLength);


	internal void SetNextBgm(string? nextBgmName, Song? nextBgm)
	{
		if (nextBgm == null)
		{
			MediaPlayer.Stop();
			return;
		}

		SetNextBgm(nextBgmName, nextBgm, TimeSpan.FromSeconds(0.6), TimeSpan.FromSeconds(0.6), TimeSpan.FromSeconds(0.3));
	}

	public void SetNextBgm(string? nextBgmName)
	{
		Song? bgmSong = null;

		if (nextBgmName != null)
		{
			bgmSong = Contents.Load<Song>(Path.Combine("Bgm", nextBgmName));
		}

		SetNextBgm(nextBgmName, bgmSong);
	}

	internal void SetNextBgm(string? nextBgmName, Song nextBgm, Curve switchingVolumeCurve, DateTime switchingBegin, DateTime switchingEnd, DateTime switchingNextBgmBegin)
	{
		_nextBgm = nextBgm;
		_nextBgmName = nextBgmName;
		SwitchingBeginTime = switchingBegin;
		SwitchingEndTime = switchingEnd;
		SwitchingNextBgmBeginTime = switchingNextBgmBegin;
		SwitchingVolumeCurve = switchingVolumeCurve;
	}

	public void SetNextBgm(string nextBgmName, Curve switchingVolumeCurve, DateTime switchingBegin, DateTime switchingEnd, DateTime switchingNextBgmBegin)
		=> SetNextBgm(nextBgmName, Contents.Load<Song>(Path.Combine("Bgm", nextBgmName)), switchingVolumeCurve, switchingBegin, switchingEnd, switchingNextBgmBegin);
	public void SwitchToNextBgmImmediately(string? nextBgmName)
	{
		if (nextBgmName != null)
		{
			Debug.Assert(nextBgmName != null);
			_currentBgm = nextBgmName;
			_currentBgmSong = Contents.Load<Song>(Path.Combine("Bgm", nextBgmName));
			_nextBgm = null;
			_nextBgmName = null;
			MediaPlayer.Play(_currentBgmSong);
		}
		else
		{
			MediaPlayer.Stop();
		}
	}

	protected internal sealed override void Update(GameTime gameTime)
	{
		var volumeBase = Volume.Value;
		var now = DateTime.Now;
		if (now > SwitchingEndTime)
		{
			MediaPlayer.Volume = volumeBase;

			if (_nextBgmName != null) // 两次更新期间跳过了背景音乐切换逻辑
				SwitchBgmToNext();
		}
		else
		{
			// 当前处于背景音乐切换过程中
			float volumeMultiplier = SwitchingVolumeCurve.Evaluate((float)(now - SwitchingBeginTime).TotalMilliseconds);
			MediaPlayer.Volume = volumeBase * volumeMultiplier;

			if (now > SwitchingNextBgmBeginTime)
			{
				SwitchBgmToNext();
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void SwitchBgmToNext() // 调用次数应该很少，给他一个NoInlining
	{
		MediaPlayer.Play(_nextBgm);

		if (_currentBgm != null)
			Contents.UnloadAsset(_currentBgm);

		_currentBgmSong = _nextBgm;
		_currentBgm = _nextBgmName;
		_nextBgm = null;
		_nextBgmName = null;
	}
}