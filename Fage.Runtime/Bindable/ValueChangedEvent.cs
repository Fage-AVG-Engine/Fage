namespace Fage.Runtime.Bindable;
/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="NewValue">设置的新值，可能不同于<see cref="BindableNumber{T}.Value"/>。</param>
/// <param name="OldValue">更新前的值</param>
public record ValueChangedEvent<T>(T NewValue, T OldValue)
{

}