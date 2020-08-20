using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace StlVault.IO.Stl
{
    internal readonly struct BinaryStlReader
    {
        private readonly Stream _stream;
        public int FacetCount { get; }

        public BinaryStlReader(Stream stream)
        {
            _stream = stream;
            FacetCount = GetFacetCount(stream);
        }

        private static int GetFacetCount(Stream file)
        {
            // Discard header
            file.Position = 80;

            Span<byte> facetCountData = stackalloc byte[4];
            file.Read(facetCountData);

            var dataFacetCount = BitConverter.ToInt32(facetCountData);
            var calculatedCount = (int)((file.Length - 84) / 50);

            if (dataFacetCount  > calculatedCount) throw new InvalidDataException("The facet count specified in the STL file is too big for the file!");            
            if (dataFacetCount != calculatedCount) Debug.WriteLine("Calculated facet count and the one in STL file don't match up!");

            return Math.Min(dataFacetCount, calculatedCount);
        }

        public FacetEnumerator GetEnumerator() => new FacetEnumerator(_stream);

        public ref struct FacetEnumerator
        {
            private static readonly int ItemSize = Unsafe.SizeOf<Facet>();

            private readonly Stream _stream;
            private readonly Facet[] _buffer;
            private readonly Span<byte> _rawBuffer;
            private bool _lastBuffer;
            private long _loadedItems;
            private int _currentItem;

            public FacetEnumerator(Stream stream)
            {
                _stream = stream;
                _buffer = ArrayPool<Facet>.Shared.Rent(2048); // alloc items buffer
                _rawBuffer = MemoryMarshal.Cast<Facet, byte>(_buffer); // cast items buffer to bytes buffer (no copies)
                _lastBuffer = false;
                _loadedItems = 0;
                _currentItem = -1;
            }            

            public ref readonly Facet Current => ref _buffer[_currentItem];

            public bool MoveNext()
            {
                // increment current position and check if reached end of buffer
                if (++_currentItem != _loadedItems) return true;

                // check if it was the last buffer
                if (_lastBuffer) return false;

                // get next buffer
                var bytesRead = _stream.Read(_rawBuffer);
                _lastBuffer = bytesRead < _rawBuffer.Length;                
                _loadedItems = bytesRead / ItemSize;
                _currentItem = 0;

                return _loadedItems != 0;
            }

            public void Dispose()
            {
                ArrayPool<Facet>.Shared.Return(_buffer);
            }
        }
    }
}