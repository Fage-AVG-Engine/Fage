namespace Fage.Runtime.Bindable;

public abstract class BindableNumber<T> : IDisposable
	where T : unmanaged, IComparable<T>, IConvertible, IEquatable<T>
{
	private T value;

	private T defaultValue = default;

	private T minValue;
	private T maxValue;

	private bool disposed;

	public event Action<ValueChangedEvent<T>>? ValueChanged;
	//public event Action<ValueChangedEvent<T>>? DefaultChanged;
	//public event Action<ValueChangedEvent<T>>? MinValueChanged;
	//public event Action<ValueChangedEvent<T>>? MaxValueChanged;
	protected BindableNumber(T defaultValue)
	{
		value = this.defaultValue = defaultValue;
	}

	public T Value
	{
		get => value;
		set
		{
			if (EqualityComparer<T>.Default.Equals(this.value, value))
				return;

			SetValue(this.value, value);
		}
	}

	internal void SetValue(T previousValue, T newValue)
	{
		value = ClampValue(newValue, minValue, maxValue);
		TriggerValueChange(previousValue, newValue);
	}

	protected abstract T ClampValue(T newValue, T minValue, T maxValue);

	protected void TriggerValueChange(T previousValue, T newValue)
	{
		ValueChanged?.Invoke(new ValueChangedEvent<T>(newValue, previousValue));
	}

	public T Default
	{
		get => defaultValue;
		set => defaultValue = value;
	}

	protected abstract T DefaultMinValue { get; }
	protected abstract T DefaultMaxValue { get; }

	public T MinValue
	{
		get => minValue;
		set => minValue = value;

	}

	public T MaxValue
	{
		get => maxValue;
		set => maxValue = value;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposed)
		{
			if (disposing)
			{
			}

			disposed = true;
		}
	}

	public void Dispose()
	{
		// 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}