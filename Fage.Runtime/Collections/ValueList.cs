using System.Collections;
using System.Runtime.CompilerServices;

namespace Fage.Runtime.Collections
{
	public struct ValueList<T> : IEnumerable<T>
		where T : unmanaged
	{
		internal int CountInternal;
		private T[] _storage;

		internal readonly T[] Storage => _storage;

		public readonly ref T this[int index]
		{
			[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref _storage[index];
		}


		public readonly int Count
		{
			[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => CountInternal;
		}

		public ValueList(int capacity)
		{
			if (capacity < 1)
				capacity = 8;

			_storage = new T[capacity];
		}

		public void Add(ref readonly T item)
		{
			if (CountInternal != _storage.Length)
			{
				_storage[CountInternal++] = item;
			}
			else
			{
				AddSlow(in item);
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private void AddSlow(ref readonly T item)
		{
			Grow();
			_storage[CountInternal++] = item;
		}

		private void Grow()
		{
			Array.Resize(ref _storage, _storage.Length * 2);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private void GrowLargeScale(int newSize)
		{
			int newCapacity = (int)Math.Ceiling(Math.Ceiling(newSize / 4.0) * 4);
			Array.Resize(ref _storage, newCapacity);
		}

		public void ClearAndResize(int newSize, bool allowShrink = false)
		{
			if (newSize != _storage.Length && (newSize > _storage.Length || allowShrink))
			{
				_storage = new T[newSize];
			}
			CountInternal = 0;
		}

		public void AddRange(ReadOnlySpan<T> collection)
		{
			int capacity = _storage.Length;
			int newSize = CountInternal + collection.Length;

			if (capacity >= newSize)
			{
				collection.CopyTo(_storage.AsSpan(CountInternal));
			}
			else
			{
				AddRangeSlow(collection, capacity, newSize);
			}
			CountInternal = newSize;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private void AddRangeSlow(ReadOnlySpan<T> collection, int capacity, int newSize)
		{
			if (capacity * 2 > newSize)
				Grow();
			else
				GrowLargeScale(newSize);

			collection.CopyTo(_storage.AsSpan(CountInternal));
		}

		public void AssignFromRange(ReadOnlySpan<T> range)
		{
			if (_storage.Length < range.Length)
				ClearAndResize(range.Length);

			range.CopyTo(_storage);
			CountInternal = range.Length;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			CountInternal = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly public ReadOnlySpan<T> AsReadonlySpan() => new(_storage, 0, CountInternal);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly public ReadOnlyMemory<T> AsReadonlyMemory() => new(_storage, 0, CountInternal);

		readonly public T[] ObtainCopy()
		{
			T[] copy = new T[CountInternal];
			AsReadonlySpan().CopyTo(copy);

			return copy;
		}

		public struct Enumerator(ValueList<T> parent) : IEnumerator<T>
		{
			public int Count = parent.CountInternal;
			private int _currentIndex = -1;
			public T[] Storage = parent._storage;

			public readonly ref T Current => ref Storage[_currentIndex];

			readonly T IEnumerator<T>.Current => Storage[_currentIndex];

			readonly object IEnumerator.Current => Current;

			public bool MoveNext()
			{
				if (_currentIndex < Count)
				{
					++_currentIndex;
					return true;
				}
				else
				{
					return false;
				}
			}

			public void Reset()
			{
				_currentIndex = -1;
			}

			public readonly void Dispose() { /* no-op */}
		}

		readonly public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		readonly IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return GetEnumerator();
		}

		readonly IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<T>)this).GetEnumerator();
		}
	}
}