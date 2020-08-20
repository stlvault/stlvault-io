using System;
using System.Buffers;
using System.Numerics;
using System.Runtime.InteropServices;

namespace StlVault.IO
{
    public class ArrayPoolBufferFactory : IMeshBufferFactory
    {
        private readonly ArrayPool<byte> _pool;

        public ArrayPoolBufferFactory()
        {
            // 0x40000000 is the maximum size in the internal implementation
            _pool = ArrayPool<byte>.Create(0x40000000, 10);
        }

        public IMeshBuffer RequestBuffer(int vertexCount, bool createNormalBuffer) => new Buffer(_pool, vertexCount, createNormalBuffer);

        private struct Buffer : IMeshBuffer
        {
            private readonly bool _hasNormalBuffer;
            private readonly ArrayPool<byte> _pool;
            private readonly byte[] _store;
            private readonly int _initialSize;
            private int _shrunkSize;

            public Buffer(ArrayPool<byte> pool, int maxLength, bool createNormalBuffer)
            {
                _hasNormalBuffer = createNormalBuffer;
                _pool = pool;

                _shrunkSize = _initialSize = GetByteSize(maxLength, _hasNormalBuffer);
                _store = _pool.Rent(_initialSize);
            }
            private static int GetByteSize(int length, bool hasNormalBuffer)
            {
                var arrayCount = hasNormalBuffer ? 2 : 1;
                return length * Marshal.SizeOf(Vector3.Zero) * arrayCount;
            }

            private int DataLength => _shrunkSize / 2;
            private int NormalOffset => _hasNormalBuffer
                ? _shrunkSize / 2
                : throw new InvalidOperationException("The buffer was not configured to store normals.");

            public Span<Vector3> VertexData => MemoryMarshal.Cast<byte, Vector3>(_store.AsSpan(0, DataLength));
            public Span<Vector3> NormalData => MemoryMarshal.Cast<byte, Vector3>(_store.AsSpan(NormalOffset, DataLength));

            public void Shrink(int targetVertexCount)
            {
                var targetSize = GetByteSize(targetVertexCount, _hasNormalBuffer);
                if (targetSize > _shrunkSize)
                {
                    throw new InvalidOperationException("Can't use shrink to grow buffer!");
                }

                _shrunkSize = targetSize;
            }
            
            public void Dispose() => _pool.Return(_store);
        }
    }
}