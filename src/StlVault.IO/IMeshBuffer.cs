using System;
using System.Numerics;

namespace StlVault.IO
{
    public interface IMeshBuffer : IDisposable
    {
        public Span<Vector3> VertexData { get; }
        public Span<Vector3> NormalData { get; }

        /// <summary>
        /// Sets the size of the actual contents of the buffer.
        /// Must be smaller than the maximum buffer size.
        /// Implementations may return the unused space.
        /// May not be used to grow a buffer.
        /// </summary>
        /// <param name="usedVertexCount">Size the buffer should be shrunk to.</param>
        void Shrink(int usedVertexCount);
    }
}