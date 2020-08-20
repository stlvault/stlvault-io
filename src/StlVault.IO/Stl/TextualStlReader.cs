using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using static System.Runtime.CompilerServices.MethodImplOptions;

namespace StlVault.IO.Stl
{
    internal readonly struct TextualStlReader
    {
        private readonly MemoryMappedFile _file;
        public TextualStlReader(FileStream stream)
        {
            _file = MemoryMappedFile.CreateFromFile(
                stream, null, 0, MemoryMappedFileAccess.Read, HandleInheritability.None, false);
        }

        public FacetEnumerator GetEnumerator() => new FacetEnumerator(_file);

        private static readonly byte[] Solid = { 0x73, 0x6F, 0x6C, 0x69, 0x64 };
        private static readonly byte[] Facet = { 0x66, 0x61, 0x63, 0x65, 0x74 };
        private static readonly byte[] Normal = { 0x6E, 0x6F, 0x72, 0x6D, 0x61, 0x6C };
        private static readonly byte[] Outer = { 0x6F, 0x75, 0x74, 0x65, 0x72 };
        private static readonly byte[] Loop = { 0x6C, 0x6F, 0x6F, 0x70 };
        private static readonly byte[] Vertex = { 0x76, 0x65, 0x72, 0x74, 0x65, 0x78 };
        private static readonly byte[] EndLoop = { 0x65, 0x6E, 0x64, 0x6C, 0x6F, 0x6F, 0x70 };
        private static readonly byte[] EndFacet = { 0x65, 0x6E, 0x64, 0x66, 0x61, 0x63, 0x65, 0x74 };
        // private static readonly byte[] EndSolid = {0x65, 0x6E, 0x64, 0x73, 0x6F, 0x6C, 0x69, 0x64};
        private static readonly byte[][] FacetNormal = { Facet, Normal };
        private static readonly byte[][] OuterLoop = { Outer, Loop };
        private static readonly byte[][] EndLoopEndFacet = { EndLoop, EndFacet };

        public unsafe ref struct FacetEnumerator
        {
            private readonly MemoryMappedViewAccessor _accessor;
            private byte* _posInFile;
            private readonly Facet[] _buffer;

            public FacetEnumerator(MemoryMappedFile file)
            {
                _posInFile = null;
                _accessor = file.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
                _accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref _posInFile);

                _buffer = new Facet[1];

                ReadHeader(ref _posInFile, out _);
            }

            public ref readonly Facet Current => ref _buffer[0];

            public bool MoveNext() => ReadFacet(ref _posInFile, ref _buffer[0]);

            public void Dispose()
            {
                _accessor.SafeMemoryMappedViewHandle.ReleasePointer();
                _accessor.Dispose();
            }
        }
        
        private static unsafe bool ReadFacet(ref byte* posInFile, ref Facet facet)
        {
            // facet = default;

            // Read normal
            if (!AdvanceWords(ref posInFile, FacetNormal)) return false;
            var normal = ReadVector3(ref posInFile);

            // Enter facet loop
            if (!AdvanceWords(ref posInFile, OuterLoop)) return false;

            // Read verts
            var vectors = stackalloc Vector3[3];
            for (var i = 0; i < 3; i++)
            {
                if (!AdvanceWord(ref posInFile, Vertex)) return false;
                vectors[i] = ReadVector3(ref posInFile);
            }

            // Exit facet loop
            if (!AdvanceWords(ref posInFile, EndLoopEndFacet)) return false;

            facet = new Facet(normal, vectors[0], vectors[1], vectors[2]);

            return true;
        }

        private static unsafe Vector3 ReadVector3(ref byte* posInFile)
        {
            var buffer = stackalloc double[3];
            for (var i = 0; i < 3; i++)
            {
                ref var number = ref buffer[i];

                SeekOverWhitespace(ref posInFile);

                var currentChar = *posInFile;
                var negative = currentChar == '-';
                if (negative || currentChar == '+') posInFile++;

                var wholePart = 0;
                while (ReadAndAdvance(ref posInFile, out currentChar))
                {
                    if (currentChar == '.') goto Fraction;

                    var currentDigit = currentChar - '0';
                    wholePart = 10 * wholePart + currentDigit;
                }
                
                Fraction:
                var fractionalPart = 0d;
                var nextFractionalDigitScale = 0.1d;
                while (ReadAndAdvance(ref posInFile, out currentChar))
                {
                    if (currentChar == 'e') goto Exponent;

                    var currentDigit = currentChar - '0';
                    fractionalPart += currentDigit * nextFractionalDigitScale;
                    nextFractionalDigitScale *= 0.1d;
                }

                // No exponent found => early exit !?
                number = wholePart + fractionalPart;
                if (negative) number *= -1;
                continue;

                Exponent:
                var exponent = 0;
                var exponentNegative = *posInFile == '-';
                if (exponentNegative || *posInFile == '+') posInFile++;

                while (ReadAndAdvance(ref posInFile, out currentChar))
                {
                    var currentDigit = currentChar - '0';
                    exponent += 10 * exponent + currentDigit;
                }

                if (exponentNegative) exponent *= -1;
                number = (wholePart + fractionalPart) * Math.Pow(10, exponent);
                if (negative) number *= -1;
            }

            return new Vector3((float)buffer[0], (float)buffer[1], (float)buffer[2]);
        }

        [MethodImpl(AggressiveInlining)]
        private static unsafe bool ReadAndAdvance(ref byte* pos, out byte currentChar)
        {
            // not a space
            var result = (currentChar = *pos) > 0x20;
            if (result) pos++;

            return result;
        }

        [MethodImpl(AggressiveInlining)]
        private static unsafe bool AdvanceWords(ref byte* posInFile, byte[][] words)
        {
            for (var i = 0; i < words.Length; i++)
            {
                ref var word = ref words[i];
                if (!AdvanceWord(ref posInFile, word)) return false;
            }

            return true;
        }

        [MethodImpl(AggressiveInlining)]
        private static unsafe bool AdvanceWord(ref byte* posInFile, byte[] word)
        {
            SeekOverWhitespace(ref posInFile);
            return CompareAdvance(ref posInFile, word);
        }

        private static unsafe void ReadHeader(ref byte* posInFile, out string solidName)
        {
            if (!CompareAdvance(ref posInFile, Solid)) throw new InvalidDataException();
            SeekOverWhitespace(ref posInFile);

            var stringStart = posInFile;
            while (*posInFile > 0x0D) posInFile++;

            solidName = Encoding.ASCII.GetString(stringStart, (int)(posInFile - stringStart));
        }

        [MethodImpl(AggressiveInlining)]
        private static unsafe void SeekOverWhitespace(ref byte* posInFile)
        {
            while (*posInFile < 0x21) posInFile++;
        }

        [MethodImpl(AggressiveInlining)]
        private static unsafe bool CompareAdvance(ref byte* posInFile, byte[] bytes)
        {
            fixed (byte* start = bytes)
            {
                for (var i = 0; i < bytes.Length; i++)
                {
                    if (*posInFile != *(start + i)) return false;
                    posInFile++;
                }
            }

            return true;
        }
    }
}