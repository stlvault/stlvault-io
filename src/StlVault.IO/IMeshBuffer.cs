using System;
using System.Numerics;

namespace StlVault.IO
{
    public interface IMeshBuffer : IDisposable
    {
        public Span<Vector3> VertexData { get; }
        public Span<Vector3> NormalData { get; }
        
        /// <summary>
        /// Allows to release parts of the reserved buffer for reuse.
        /// This can be used if the buffer was over-provisioned.
        /// </summary>
        /// <param name="targetVertexCount">The target size the buffer should be shrunk to.</param>
        public void Shrink(int targetVertexCount);
    }
}