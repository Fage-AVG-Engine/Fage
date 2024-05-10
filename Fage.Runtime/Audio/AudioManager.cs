using Fage.Runtime.Bindable;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using SharpDX.Direct3D9;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Fage.Runtime.Audio;

public class AudioManager : IDisposable
{
	private bool disposedValue;

	// public new FageTemplateGame Game => (FageTemplateGame)base.Game;
	public ContentManager Contents { get; private set; }
	public AudioManager(Game game)
	{
		MediaPlayer.IsRepeating = true;
		Contents = new(game.Services, Path.Combine(game.Content.RootDirectory, "Audio"));

		BackgroundMusicChannel = new(this);
		VoiceChannel = new(this);
		SfxChannel = new(this);
	}

	#region 音量

	public BindableFloat BgmVolume { get; } = new(0.6f)
	{
		MinValue = 0,
		MaxValue = 1
	};

	public BindableFloat VoiceVolume { get; } = new(0.6f)
	{
		MinValue = 0,
		MaxValue = 1
	};

	public BindableFloat SfxVolume { get; } = new(0.6f)
	{
		MinValue = 0,
		MaxValue = 1
	};

	#endregion

	#region 通道
	/// <summary>
	/// 背景音效通道
	/// </summary>
	public BackgroundMusicChannel BackgroundMusicChannel { get; protected set; }

	/// <summary>
	/// 语音通道
	/// </summary>
	public VoiceChannel VoiceChannel { get; protected set; }

	/// <summary>
	/// 音效通道
	/// </summary>
	public SfxChannel SfxChannel { get; protected set; }
	#endregion

	public void Update(GameTime gameTime)
	{
		BackgroundMusicChannel.Update(gameTime);
		VoiceChannel.Update(gameTime);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				VoiceChannel.UnloadResources();
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