using Fage.Runtime.Layering;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;

namespace Fage.Runtime.Layers;

/// <summary>
/// 拦截指定来源的输入，并停止其传播
/// </summary>
/// <param name="name"></param>
/// <param name="mode"></param>
public class InputMaskLayer(string name, InputMaskMode mode) :
	ILayer, ILayeredMouseHandler, ILayeredKeyboardHandler, ILayeredTouchHandler, ILayeredGamepadHandler
{
	public string Name { get; } = name;
	public InputMaskMode Mode { get; set; } = mode;
	public ILayer? Parent { get; set; }

	public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
	{
		
	}

	public bool HandleInput(ILayer sender, LayeredMouseEventArgs e)
	{
		if (Mode.HasFlags(InputMaskMode.Mouse | InputMaskMode.Touch | InputMaskMode.OverrideMouseByTouch))
			return true;

		if (Mode.HasFlags(InputMaskMode.Mouse))
			return true;

		return false;
	}

	public bool HandleInput(ILayer sender, LayeredKeyboardEventArgs e)
	{
		if (Mode.HasFlags(InputMaskMode.Keyboard))
			return true;

		return false;
	}

	public bool HandleInput(ILayer sender, LayeredTouchEventArgs e)
	{
		if (Mode.HasFlags(InputMaskMode.Touch))
			return true;

		return false;
	}

	public bool HandleInput(ILayer sender, LayeredGamepadEventArgs e)
	{
		if (Mode.HasFlags(InputMaskMode.Gamepad))
			return true;

		return false;
	}

	public void Update(GameTime gameTime)
	{
		
	}
}

[Flags]
public enum InputMaskMode
{
	None = 0,
	Mouse = 1,
	Keyboard = 2,
	Touch = 4,
	Gamepad = 8,
	OverrideMouseByTouch = 16,

	AllSources = 15,
}

file static class ModeExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool HasFlags(this InputMaskMode value, InputMaskMode requiredFlags)
	{
		return (value & requiredFlags) == requiredFlags;
	}
}