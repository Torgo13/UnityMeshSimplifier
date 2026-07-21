#region License
/*
MIT License

Copyright(c) 2017-2020 Mattias Edlund

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion

using System;
using System.Runtime.CompilerServices;

#if OPTIMISATION_IDISPOSABLE
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
#endif // OPTIMISATION_IDISPOSABLE

namespace UnityMeshSimplifier
{
    /// <summary>
    /// A resizable array with the goal of being quicker than <see cref="System.Collections.Generic.List{T}"/>.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
#if OPTIMISATION_IDISPOSABLE
    internal struct ResizableArray<T>
        : IEnumerable<T>, IEquatable<ResizableArray<T>>, INativeDisposable where T : unmanaged
    {
        #region Fields
        internal NativeList<T> _resizableArray;
        #endregion

        #region Properties
        public readonly bool IsNull => !IsNotNull;
        public readonly bool IsNotNull => _resizableArray.IsCreated;
        public readonly bool IsEmpty => _resizableArray.IsEmpty;
        public readonly bool IsNotEmpty => !IsEmpty;

        public readonly int Length => _resizableArray.IsEmpty ? 0 : _resizableArray.Length;
        public readonly int Count => Length;
        public readonly ResizableArray<T> Data => this;
        public ref T this[int index] => ref _resizableArray.ElementAt(index);
        #endregion

        #region Constructor
        public ResizableArray(int initialCapacity, AllocatorManager.AllocatorHandle allocator)
        {
            _resizableArray = new NativeList<T>(initialCapacity, allocator);
        }

        public ResizableArray(int initialCapacity)
        {
            _resizableArray = new NativeList<T>(initialCapacity, Allocator.Domain);
        }

        public ResizableArray(int initialCapacity, int initialLength)
        {
            _resizableArray = new NativeList<T>(initialCapacity, Allocator.Domain);
            _resizableArray.Resize(initialLength, NativeArrayOptions.ClearMemory);
        }

        ResizableArray(NativeList<T> resizableArray)
        {
            _resizableArray = resizableArray;
        }

        public ResizableArray(T[] resizableArray)
        {
            _resizableArray = new NativeList<T>(resizableArray.Length, Allocator.Domain);
            foreach (T item in resizableArray)
            {
                _resizableArray.AddNoResize(item);
            }
        }
        #endregion

        #region Public Methods
        public void Clear() => _resizableArray.Clear();
        public void Add(in T value) => _resizableArray.Add(value);
        public void AddRange(T[] value)
        {
            var required = _resizableArray.Length + value.Length;
            if (_resizableArray.Capacity < required)
                _resizableArray.Capacity = required;

            foreach (var item in value)
            {
                _resizableArray.AddNoResize(item);
            }
        }
        public void ResizeUninitialized(int length) => _resizableArray.ResizeUninitialized(length);
        public void TrimExcess() => _resizableArray.TrimExcess();
        public NativeArray<T> AsArray() => _resizableArray.AsArray();
        public T[] ToArray() => _resizableArray.IsEmpty ? Array.Empty<T>() : _resizableArray.AsArray().ToArray();
        public Span<T> AsSpan() => _resizableArray.IsEmpty ? (Span<T>)Array.Empty<T>() : (Span<T>)_resizableArray.AsArray();
        public ReadOnlySpan<T> AsReadOnlySpan() => _resizableArray.IsEmpty ? (ReadOnlySpan<T>)Array.Empty<T>() : (ReadOnlySpan<T>)_resizableArray.AsArray();

        public void Resize(int length, bool trimExcess = false, bool clearMemory = false)
        {
            if (!_resizableArray.IsCreated)
                return;

            _resizableArray.Resize(length, clearMemory ? NativeArrayOptions.ClearMemory : NativeArrayOptions.UninitializedMemory);
            if (trimExcess)
            {
                _resizableArray.TrimExcess();
            }
        }
        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public static implicit operator ResizableArray<T>(UnityEngine.Object? obj) => default;
        public static implicit operator ResizableArray<T>(NativeList<T> resizableArray) => new ResizableArray<T>(resizableArray);
        public static implicit operator NativeList<T>(ResizableArray<T> resizableArray) => resizableArray._resizableArray;
        public static explicit operator NativeArray<T>(ResizableArray<T> resizableArray) => resizableArray.AsArray();
        public static implicit operator Span<T>(ResizableArray<T> resizableArray) => resizableArray._resizableArray.IsEmpty ? new Span<T>() : resizableArray.AsArray().AsSpan();
        public static implicit operator ReadOnlySpan<T>(ResizableArray<T> resizableArray) => resizableArray._resizableArray.IsEmpty ? new ReadOnlySpan<T>() : resizableArray.AsArray().AsReadOnlySpan();
        public static implicit operator T[](ResizableArray<T> resizableArray) => resizableArray.ToArray();

        #region INativeDisposable
        public Unity.Jobs.JobHandle Dispose(Unity.Jobs.JobHandle inputDeps) => _resizableArray.Dispose(inputDeps);
        public void Dispose() => _resizableArray.Dispose();
        #endregion // INativeDisposable

        #region IEquatable
        public static bool operator ==(ResizableArray<T> customArray, ResizableArray<T> other)
        {
            if (!customArray._resizableArray.IsCreated
                && !other._resizableArray.IsCreated)
                return true;

            if (customArray._resizableArray.IsCreated
                != other._resizableArray.IsCreated)
                return false;

            return customArray._resizableArray.AsArray() == other._resizableArray.AsArray();
        }
        public static bool operator !=(ResizableArray<T> customArray, ResizableArray<T> other) => !(customArray == other);
        public readonly bool Equals(ResizableArray<T> other) => this == other;
        public readonly override bool Equals(object? obj) => obj is ResizableArray<T> other && this == other;
        public override int GetHashCode() => _resizableArray.IsEmpty ? 0 : _resizableArray.AsArray().GetHashCode();
        #endregion // IEquatable

        #region IEnumerable
        public IEnumerator<T> GetEnumerator() => _resizableArray.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _resizableArray.GetEnumerator();
        #endregion // IEnumerable
    }
#else
    internal sealed class ResizableArray<T>
    {
        #region Fields
        private T[] items;
        private int length = 0;

        readonly
        private static T[] emptyArr = new T[0];
        #endregion

        #region Properties
        public bool IsNull => items.Length == 0;
        public bool IsNotNull => !IsNull;
        public bool IsEmpty => IsNull;
        public bool IsNotEmpty => !IsEmpty;

        /// <summary>
        /// Gets the length of this array.
        /// </summary>
        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return length; }
        }

        /// <summary>
        /// Gets the internal data buffer for this array.
        /// </summary>
        public T[] Data
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return items; }
        }

        /// <summary>
        /// Gets or sets the element value at a specific index.
        /// </summary>
        /// <param name="index">The element index.</param>
        /// <returns>The element value.</returns>
        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return items[index]; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { items[index] = value; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new resizable array.
        /// </summary>
        /// <param name="capacity">The initial array capacity.</param>
        public ResizableArray(int capacity)
            : this(capacity, 0)
        {

        }

        /// <summary>
        /// Creates a new resizable array.
        /// </summary>
        /// <param name="capacity">The initial array capacity.</param>
        /// <param name="length">The initial length of the array.</param>
        public ResizableArray(int capacity, int length)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));
            else if (length < 0 || length > capacity)
                throw new ArgumentOutOfRangeException(nameof(length));

            if (capacity > 0)
                items = new T[capacity];
            else
                items = emptyArr;

            this.length = length;
        }

        /// <summary>
        /// Creates a new resizable array.
        /// </summary>
        /// <param name="initialArray">The initial array.</param>
        public ResizableArray(T[] initialArray)
        {
            if (initialArray == null)
                throw new ArgumentNullException(nameof(initialArray));

            if (initialArray.Length > 0)
            {
                items = new T[initialArray.Length];
                length = initialArray.Length;
                Array.Copy(initialArray, 0, items, 0, initialArray.Length);
            }
            else
            {
                items = emptyArr;
                length = 0;
            }
        }
        #endregion

        #region Private Methods
        private void IncreaseCapacity(int capacity)
        {
            T[] newItems = new T[capacity];
            Array.Copy(items, 0, newItems, 0, System.Math.Min(length, capacity));
            items = newItems;
        }
        #endregion

        public static implicit operator Span<T>(ResizableArray<T> resizableArray) => resizableArray.AsSpan();
        public static implicit operator ReadOnlySpan<T>(ResizableArray<T> resizableArray) => resizableArray.AsReadOnlySpan();

        #region Public Methods
        public Span<T> AsSpan() => items.AsSpan(start: 0, length);
        public ReadOnlySpan<T> AsReadOnlySpan() => new ReadOnlySpan<T>(items, start: 0, length);

        /// <summary>
        /// Clears this array.
        /// </summary>
        public void Clear()
        {
            Array.Clear(items, 0, length);
            length = 0;
        }

        /// <summary>
        /// Resizes this array.
        /// </summary>
        /// <param name="length">The new length.</param>
        /// <param name="trimExess">If excess memory should be trimmed.</param>
        /// <param name="clearMemory">If memory that is no longer part of the array should be cleared.</param>
        public void Resize(int length, bool trimExess = false, bool clearMemory = false)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            if (length > items.Length)
            {
                IncreaseCapacity(length);
            }
            else if (length < this.length && clearMemory)
            {
                Array.Clear(items, length, this.length - length);
            }

            this.length = length;

            if (trimExess)
            {
                TrimExcess();
            }
        }

        /// <summary>
        /// Trims any excess memory for this array.
        /// </summary>
        public void TrimExcess()
        {
            if (items.Length == length) // Nothing to do
                return;

            var newItems = new T[length];
            Array.Copy(items, 0, newItems, 0, length);
            items = newItems;
        }

        /// <summary>
        /// Adds a new item to the end of this array.
        /// </summary>
        /// <param name="item">The new item.</param>
        public void Add(T item)
        {
            if (length >= items.Length)
            {
                IncreaseCapacity(items.Length << 1);
            }

            items[length++] = item;
        }

        /// <summary>
        /// Returns a copy of the resizable array as an actual array.
        /// </summary>
        /// <returns>The array.</returns>
        public T[] ToArray()
        {
            var newItems = new T[length];
            Array.Copy(items, 0, newItems, 0, length);
            return newItems;
        }
        #endregion
    }
#endif // OPTIMISATION_IDISPOSABLE
}
