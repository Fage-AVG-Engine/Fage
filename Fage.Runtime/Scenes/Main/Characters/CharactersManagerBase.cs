using Fage.Runtime.Collections;
using Fage.Runtime.Layering;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Fage.Runtime.Scenes.Main.Characters;

public abstract class CharactersManagerBase<TCharacter>(FageTemplateGame game)
	: GameComponent(game), ILayer
	where TCharacter : Character
{
	#region 图形资源
	public Dictionary<string, TCharacter> Resources { get; protected set; } = [];
	protected GraphicsDevice GraphicsDevice => Game.GraphicsDevice;
	public ContentManager Contents { get; } = new ContentManager(game.Services, game.Content.RootDirectory);

	public string ResourcesSearchPath { get; set; } = "Sprites/Characters";

	private bool disposedValue = false;
	#endregion

	#region 状态
	public CharactersLayout Layout = new();
	protected List<TCharacter> CharactersOnScreen { get; } = [];
	protected ValueList<Rectangle> CharacterRectangles = new(4);

	#endregion

	#region 图层

	public string Name { get; } = "characters container";
	public ILayer? Parent { get; set; }

	#endregion

	protected void EnsureNotDisposed()
		=> ObjectDisposedException.ThrowIf(disposedValue, this);

	#region 资源管理
	/// <summary>
	/// 加载角色对应的资源
	/// </summary>
	/// <param name="characterIdentity"></param>
	public TCharacter GetCharacterResources(string characterIdentity)
	{
		EnsureNotDisposed();
		if (Resources.TryGetValue(characterIdentity, out TCharacter? value))
			return value;

		var res = LoadCharacterResources(characterIdentity);
		Resources.Add(characterIdentity, res);

		return res;
	}

	protected abstract TCharacter LoadCharacterResources(string characterIdentity);

	protected virtual void TryUnloadCharacterResources(string identity, TCharacter toBeRemove)
	{
		toBeRemove.UnloadResources(Contents);
		Resources.Remove(identity);
	}

	/// <summary>
	/// 进行幕间资源清理
	/// </summary>
	/// <remarks>
	/// 可以在一段脚本完成后调用，清理下一章用不到的资源
	/// </remarks>
	/// <param name="identityWillUse">
	/// 下一章还要用到的角色们的标识
	/// </param>
	public virtual void InterludeCleanup(string[] identityWillUse)
	{
		Dictionary<string, TCharacter> loadedCharacters = Resources;

#if !NETCOREAPP3_0_OR_GREATER // 真的有人要用吗
		if (CheckDotnetVersion.RunningOnCore3AndLater)
		{
#endif
		foreach (var character in loadedCharacters)
		{
			if (identityWillUse.Contains(character.Key)
				|| CharactersOnScreen.Contains(character.Value))
			{
				// 不用卸载
			}
			else
			{
				character.Value.UnloadResources(Contents);
				loadedCharacters.Remove(character.Key);
			}
		}
#if !NETCOREAPP3_0_OR_GREATER
		}
		else
		{
			InterludeCleanupOld();
		}
#endif
	}

#if !NETCOREAPP3_0_OR_GREATER
	private void InterludeCleanupOld()
	{
		var toUnload = Resources.Where(kv => !CharactersOnScreen.Contains(kv.Value));
		var toKeep = Resources.Where(kv => CharactersOnScreen.Contains(kv.Value));

		foreach (var character in toUnload)
		{
			character.Value.UnloadResources(Contents);
		}

		Resources = toKeep.ToDictionary();
	}
#endif

	public virtual void LoadContent()
	{
		GraphicsDevice.DeviceReset += OnDeviceReset;
	}

	public virtual void UnloadResources()
	{
		if (!disposedValue)
		{
			GraphicsDevice.DeviceReset -= OnDeviceReset;
			Contents.Unload();
		}
	}
	#endregion

	#region 绘制

	private void OnDeviceReset(object? sender, EventArgs e)
	{
		LayoutCharacters(CharactersOnScreen);
	}

	// 测试需要 internal
	internal void TestLayoutCharacters(List<TCharacter> charactersOnScreen) => LayoutCharacters(charactersOnScreen);

	protected void LayoutCharacters(List<TCharacter> charactersOnScreen)
	{
		CharacterRectangles.Clear();
		int totalWidth = 0;
		Rectangle currentLocation = default;

		foreach (var character in charactersOnScreen)
		{
			currentLocation.X = totalWidth;
			var preferredSize = character.PreferredSize;

			int ascent = (int)(preferredSize.Y * Layout.AscentPercent);
			currentLocation.Y = GraphicsDevice.Viewport.Height - Layout.Baseline - ascent;
			currentLocation.Height = preferredSize.Y;
			currentLocation.Width = preferredSize.X;

			totalWidth += currentLocation.Width;
			totalWidth += Layout.HorizontalGap;

			CharacterRectangles.Add(ref currentLocation);
		}

		totalWidth -= Layout.HorizontalGap;

		int xOffset = (GraphicsDevice.Viewport.Width - totalWidth) / 2;
		foreach (ref Rectangle charRect in CharacterRectangles)
		{
			charRect.X += xOffset;
		}
	}

	public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);
	#endregion

	#region API

	public int CountOfOnscreenCharacters => CharactersOnScreen.Count;

	/// <summary>
	/// 将角色形象添加到屏幕上。
	/// </summary>
	/// <param name="identity">角色标识。标识的解析规则由子类确定。</param>
	/// <param name="motion">角色的表情。传入<see langword="null"/>时，使用默认表情。</param>
	/// <param name="position">
	/// 被添加角色的形象的位置。是一个以0开始，从左往右递增的数字。传入<see langword="null"/>时，角色形象将被放置在最右边。
	/// <para>例如1代表从左往右数第二个位置。</para>
	/// </param>
	public void AddCharacter(string identity, string? motion = null, int? position = default)
	{
		AddCharacterImmediately(identity);
	}

	/// <summary>
	/// 将角色形象添加到屏幕上，没有过渡动画。
	/// </summary>
	/// <param name="identity">角色标识。标识的解析规则由子类确定。</param>
	/// <param name="motion">角色的表情。传入<see langword="null"/>时，使用默认表情。</param>
	/// <param name="position">
	/// 被添加角色的形象的位置。是一个以0开始，从左往右递增的数字。传入<see langword="null"/>时，角色形象将被放置在最右边。
	/// <para>例如1代表从左往右数第二个位置。</para>
	/// </param>
	public void AddCharacterImmediately(string identity, string? motion = null, int? position = null)
	{
		var toBeAdded = GetCharacterResources(identity);
		if (CharactersOnScreen.Contains(toBeAdded))
			throw new InvalidOperationException("不支持多次向屏幕上添加标识相同的角色。" +
				"如果有此类需要建议使用赝品（即内容相同，标识不同）。");

		toBeAdded.Motion = motion ?? Character.DefaultMotionName;

		if (position.HasValue)
		{
			if (position < 0)
				position = 0;
			else if (position > CharactersOnScreen.Count)
				position = CharactersOnScreen.Count;

			CharactersOnScreen.Insert(position.Value, toBeAdded);
		}
		else
		{
			CharactersOnScreen.Add(toBeAdded);
		}

		LayoutCharacters(CharactersOnScreen);
	}

	public void RemoveCharacter(string identity, bool tryUnloadResource = false)
	{
		RemoveCharacterImmediately(identity, tryUnloadResource);
	}

	/// <summary>
	/// 将角色形象从屏幕上移除。
	/// </summary>
	/// <param name="identity">角色标识。</param>
	/// <param name="tryUnloadResources">是否尝试卸载资源。</param>
	public void RemoveCharacterImmediately(string identity, bool tryUnloadResources = true)
	{
		var toBeRemove = GetCharacterResources(identity);
		var index = CharactersOnScreen.IndexOf(toBeRemove);

		if (index != -1)
		{
			if (tryUnloadResources)
			{
				TryUnloadCharacterResources(identity, toBeRemove);
			}
			CharactersOnScreen.RemoveAt(index);
			LayoutCharacters(CharactersOnScreen);
		}
	}
	#endregion

	protected override void Dispose(bool disposing)
	{
		if (disposedValue) return;

		if (disposing)
		{
			UnloadResources();
		}
		
		base.Dispose(disposing);
		disposedValue = true;
	}
}