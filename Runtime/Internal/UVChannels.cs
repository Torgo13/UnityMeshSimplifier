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

using System.Runtime.CompilerServices;

#if OPTIMISATION
using static UnityMeshSimplifier.MeshUtils;
#endif // OPTIMISATION

namespace UnityMeshSimplifier.Internal
{
    sealed
    internal class UVChannels<TVec>
#if OPTIMISATION_IDISPOSABLE
        : System.IDisposable where TVec : unmanaged
#endif // OPTIMISATION_IDISPOSABLE
    {
#if OPTIMISATION
        private readonly ResizableArray<TVec>?[] channels;
        private readonly TVec[]?[] channelsData;
#else
        private static readonly int UVChannelCount = MeshUtils.UVChannelCount;

        private ResizableArray<TVec>[] channels = null;
        private TVec[][] channelsData = null;
#endif // OPTIMISATION

        public TVec[]?[] Data
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                for (int i = 0; i < UVChannelCount; i++)
                {
                    channelsData[i] = channels[i]?.Data ?? null;
                }
                return channelsData;
            }
        }

        /// <summary>
        /// Gets or sets a specific channel by index.
        /// </summary>
        /// <param name="index">The channel index.</param>
        public ResizableArray<TVec>? this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return channels[index]; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { channels[index] = value; }
        }

        public UVChannels()
        {
            channels = new ResizableArray<TVec>?[UVChannelCount];
            channelsData = new TVec[UVChannelCount][];
        }

#if OPTIMISATION_IDISPOSABLE
        public void Dispose()
        {
            foreach (var channel in channels)
            {
                if (channel.HasValue)
                    channel.Value.Dispose();
            }
        }
#endif // OPTIMISATION_IDISPOSABLE

        /// <summary>
        /// Resizes all channels at once.
        /// </summary>
        /// <param name="capacity">The new capacity.</param>
        /// <param name="trimExcess">If excess memory should be trimmed.</param>
        public void Resize(int capacity, bool trimExcess = false)
        {
            for (int i = 0; i < UVChannelCount; i++)
            {
                channels[i]?.Resize(capacity, trimExcess);
            }
        }
    }
}
