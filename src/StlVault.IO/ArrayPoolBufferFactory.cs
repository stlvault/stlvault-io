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
            private readonly ArrayPool<byte> _pool;
            private readonly byte[] _store;
            private readonly bool _hasNormalBuffer;
            private readonly int _bufferSize;
            private int _usedSize;

            public Buffer(ArrayPool<byte> pool, int maxLength, bool createNormalBuffer)
            {
                _bufferSize = maxLength * Marshal.SizeOf(Vector3.Zero);
                _hasNormalBuffer = createNormalBuffer;
                _usedSize = _bufferSize;
                _pool = pool;

                lock (_pool)
                {
                    var arrayCount = createNormalBuffer ? 2 : 1;
                    _store = _pool.Rent(_bufferSize * arrayCount);
                }
            }
            
            public Span<Vector3> VertexData => MemoryMarshal.Cast<byte, Vector3>(_store.AsSpan(0, _usedSize));
            public Span<Vector3> NormalData => MemoryMarshal.Cast<byte, Vector3>(_hasNormalBuffer ? _store.AsSpan(_bufferSize, _usedSize) : Span<byte>.Empty);

            public void Shrink(int usedVertexCount)
            {
                var usedSize = usedVertexCount * Marshal.SizeOf(Vector3.Zero);
                if (usedSize > _usedSize)
                {
                    throw new InvalidOperationException("Buffer may not be grown using Shrink!");
                }

                _usedSize = usedSize;
            }

            public void Dispose()
            {
                lock (_pool)
                {
                    _pool.Return(_store);
                }
            }
        }
    }
}