using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Fage.Runtime.UI;

[Flags]
public enum UIButtonState
{
	Released = 0,
	Hovering = 1,
	Pressed = 2,
	Disabled = 4,

#pragma warning disable CA1069 // 不应复制枚举值
	[EditorBrowsable(EditorBrowsableState.Never)]
	FlagsNone = 0
#pragma warning restore CA1069 // 这是Flag，出于语义清晰起见，应该有一个None的值为0
}

[EditorBrowsable(EditorBrowsableState.Advanced)]
public static class ButtonStateExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsReleased(this UIButtonState state)
	{
		return state == UIButtonState.Released;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsHovering(this UIButtonState state)
	{
		return (state & UIButtonState.Hovering) != UIButtonState.FlagsNone;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsPressed(this UIButtonState state)
	{
		return (state & UIButtonState.Pressed) != UIButtonState.FlagsNone;
	}
}