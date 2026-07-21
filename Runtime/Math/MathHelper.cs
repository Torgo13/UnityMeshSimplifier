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

namespace UnityMeshSimplifier
{
    /// <summary>
    /// Math helpers.
    /// </summary>
    public static class MathHelper
    {
        #region Consts
        /// <summary>
        /// The Pi constant.
        /// </summary>
        public const float PI = (float)PId;

        /// <summary>
        /// The Pi constant.
        /// </summary>
        public const double PId = 3.1415926535897932384626433832795;

        /// <summary>
        /// Degrees to radian constant.
        /// </summary>
        public const float Deg2Rad = PI / 180f;

        /// <summary>
        /// Degrees to radian constant.
        /// </summary>
        public const double Deg2Radd = PId / 180.0;

        /// <summary>
        /// Radians to degrees constant.
        /// </summary>
        public const float Rad2Deg = 180f / PI;

        /// <summary>
        /// Radians to degrees constant.
        /// </summary>
        public const double Rad2Degd = 180.0 / PId;
        #endregion

        #region Min
        /// <summary>
        /// Returns the minimum of three values.
        /// </summary>
        /// <param name="val1">The first value.</param>
        /// <param name="val2">The second value.</param>
        /// <param name="val3">The third value.</param>
        /// <returns>The minimum value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Min(double val1, double val2, double val3)
        {
            return (val1 < val2 ? (val1 < val3 ? val1 : val3) : (val2 < val3 ? val2 : val3));
        }
        #endregion

        #region Clamping
        /// <summary>
        /// Clamps a value between a minimum and a maximum value.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <returns>The clamped value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Clamp(double value, double min, double max)
        {
            return (value >= min ? (value <= max ? value : max) : min);
        }
        #endregion

        #region Triangle Area
        /// <summary>
        /// Calculates the area of a triangle.
        /// </summary>
        /// <param name="p0">The first point.</param>
        /// <param name="p1">The second point.</param>
        /// <param name="p2">The third point.</param>
        /// <returns>The triangle area.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double TriangleArea(ref Vector3d p0, ref Vector3d p1, ref Vector3d p2)
        {
            var dx = p1 - p0;
            var dy = p2 - p0;
            return dx.Magnitude * (Math.Sin(Vector3d.Angle(ref dx, ref dy) * Deg2Radd) * dy.Magnitude) * 0.5f;
        }
        #endregion
    }
}

// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License
namespace UnityEngine
{
    /// <summary>
    /// Some helpers to handle <see cref="System.Collections.Generic.List{T}"/> in C# API (used for no-alloc APIs where user provides a list):
    ///   On IL2CPP/Mono we can "resize" <see cref="System.Collections.Generic.List{T}"/> (up to Capacity, sure, but this is/should-be handled higher level)
    ///   Also we can easily "convert" <see cref="System.Collections.Generic.List{T}"/> to <see cref="System.Array"/>
    /// </summary>
    /// <remarks>NB .NET backend is treated as second-class citizen going through <see cref="System.Collections.Generic.List{T}.ToArray"/> call</remarks>
    internal static class NoAllocHelpers
    {
        public static void EnsureListElemCount<T>(System.Collections.Generic.List<T> list, int count)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            if (count < 0)
                throw new ArgumentException("invalid size to resize.", nameof(list));

            list.Clear();

            // make sure capacity is enough (that's where alloc WILL happen if needed)
            if (list.Capacity < count)
                list.Capacity = count;

            if (count != list.Count)
            {
                ListPrivateFieldAccess<T> tListAccess = Unity.Collections.LowLevel.Unsafe.UnsafeUtility.As<System.Collections.Generic.List<T>, ListPrivateFieldAccess<T>>(ref list);
                tListAccess._size = count;
                tListAccess._version++;
            }
        }

        // tiny helpers
        public static int SafeLength(Array? values) { return values != null ? values.Length : 0; }
        public static int SafeLength<T>(System.Collections.Generic.List<T>? values) { return values != null ? values.Count : 0; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[]? ExtractArrayFromList<T>(System.Collections.Generic.List<T>? list)
        {
            if (list == null)
                return null;

            var tListAccess = Unity.Collections.LowLevel.Unsafe.UnsafeUtility.As<System.Collections.Generic.List<T>, ListPrivateFieldAccess<T>>(ref list);
            return tListAccess._items;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> CreateSpan<T>(System.Collections.Generic.List<T>? list)
        {
            if (list == null)
                return default;

            var tListAccess = Unity.Collections.LowLevel.Unsafe.UnsafeUtility.As<System.Collections.Generic.List<T>, ListPrivateFieldAccess<T>>(ref list);
            return new Span<T>(tListAccess._items, 0, tListAccess._size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> CreateReadOnlySpan<T>(System.Collections.Generic.List<T>? list)
        {
            if (list == null)
                return default;

            var tListAccess = Unity.Collections.LowLevel.Unsafe.UnsafeUtility.As<System.Collections.Generic.List<T>, ListPrivateFieldAccess<T>>(ref list);
            return new ReadOnlySpan<T>(tListAccess._items, 0, tListAccess._size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ResetListSize<T>(System.Collections.Generic.List<T> list, int size)
        {
            if (list.Capacity < size) throw new ArgumentException($"Resetting to {size} which is bigger than capacity {list.Capacity} is not allowed!");

            var tListAccess = Unity.Collections.LowLevel.Unsafe.UnsafeUtility.As<System.Collections.Generic.List<T>, ListPrivateFieldAccess<T>>(ref list);

            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>() && tListAccess._size > size)
                Array.Clear(tListAccess._items, size, tListAccess._size - size);

            tListAccess._size = size;
            tListAccess._version++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InvalidateListEnumerators<T>(System.Collections.Generic.List<T> list)
        {
            if (list.Count == 0)
                return;

            // Updating an element causes the version to be updated
            list[0] = list[0];
        }

        /// <summary>
        /// This is a helper class to allow the binding code to manipulate the internal fields of
        /// <see cref="System.Collections.Generic.List{T}"/>. The field order below must not be changed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        [Scripting.Preserve]
        private class ListPrivateFieldAccess<T>
        {
#pragma warning disable CS0649
#pragma warning disable CS8618
            internal T[] _items; // Do not rename (binary serialization)
#pragma warning restore CS8618
            internal int _size; // Do not rename (binary serialization)
            internal int _version; // Do not rename (binary serialization)
#pragma warning restore CS0649
        }
    }

    public static class ListExtensions
    {
        public static T[]? AsArray<T>(this System.Collections.Generic.List<T> list)
            => NoAllocHelpers.ExtractArrayFromList(list);
        public static Span<T> AsSpan<T>(this System.Collections.Generic.List<T> list)
            => NoAllocHelpers.CreateSpan(list);
        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this System.Collections.Generic.List<T> list)
            => NoAllocHelpers.CreateReadOnlySpan(list);
        public static void EnsureCount<T>(this System.Collections.Generic.List<T> list, int count)
            => NoAllocHelpers.EnsureListElemCount(list, count);
    }
}
