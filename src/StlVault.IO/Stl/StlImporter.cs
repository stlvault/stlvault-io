using System;
using System.Buffers;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using static System.Runtime.CompilerServices.MethodImplOptions;

namespace StlVault.IO.Stl
{
    public static class StlImporter
    {
        private const int MinimumAsciiFacetChars = 80;

        /// <summary>
        /// Tries to determine which kind of STL we are dealing with.
        /// STL files exist in two flavors:
        ///   - Textual STLs using ASCII Encoding
        ///   - Binary STLs with an 80 byte header, 4 byte facet count, n*50 byte facets
        /// </summary>
        public static bool IsBinary(FileStream fileStream)
        {
            // Minimum length for header + one facet
            if (fileStream.Length < 130) return false;

            var fileBytes = ArrayPool<byte>.Shared.Rent(130);
            try
            {
                fileStream.Read(fileBytes);

                for (var i = 0; i < 80; i++)
                {
                    // Null bytes should be used for empty header bytes
                    if (fileBytes[i] == 0x0)
                    {
                        return true;
                    }
                }

                for (var i = 80; i < 130; i++)
                {
                    // Chars outside of ASCII range are likely for binary files
                    if (fileBytes[i] > 126) return true;
                }

                // According to spec this should no be the case for binary files!
                // But nobody seems to care - so this is a last ditch effort..
                return Encoding.ASCII.GetString(fileBytes, 0, 6) != "solid ";
            }
            finally
            {
                fileStream.Position = 0;
                ArrayPool<byte>.Shared.Return(fileBytes);
            }
        }

        public static IMeshBuffer ImportMesh(string path, IMeshBufferFactory factory)
        {
            var fileName = Path.GetFileName(path);

            using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (stream.Length < 112) throw new InvalidDataException($"File `{fileName}` is too small to be any kind of valid STL.");

            IMeshBuffer? buffer = null;
            try
            {
                if (IsBinary(stream))
                {
                    var reader = new BinaryStlReader(stream);
                    buffer = factory.RequestBuffer(reader.FacetCount * 3, createNormalBuffer: true);
                    WriteMeshData(reader, buffer);
                }
                else
                {
                    var facets = new TextualStlReader(stream);
                    buffer = factory.RequestBuffer((int) (stream.Length / MinimumAsciiFacetChars), createNormalBuffer: true);
                    var actualSize = WriteMeshData(facets, buffer);
                    buffer.Shrink(actualSize);
                }
            }
            catch
            {
                buffer?.Dispose();
                throw;
            }

            return buffer;
        }


        private static void WriteMeshData(BinaryStlReader input, IMeshBuffer output)
        {
            var vertexData = output.VertexData;
            var normalData = output.NormalData;

            var i = 0;
            foreach (ref readonly var facet in input)
            {
                WriteFacet(vertexData, normalData, in facet, i);
                i += 3;
            }
        }

        private static int WriteMeshData(TextualStlReader input, IMeshBuffer output)
        {
            var vertexData = output.VertexData;
            var normalData = output.NormalData;

            var f = 0;
            foreach (ref readonly var facet in input)
            {
                WriteFacet(vertexData, normalData, in facet, f);
                f += 3;
            }

            return f;
        }

        [MethodImpl(AggressiveInlining)]
        private static void WriteFacet(Span<Vector3> vertSpan, Span<Vector3> normSpan, in Facet facet, int offset)
        {
            // Invert indices because .stl files are right-handed
            var a = new Vector3(-facet.vert_1.Y, facet.vert_1.Z, facet.vert_1.X);
            var b = new Vector3(-facet.vert_2.Y, facet.vert_2.Z, facet.vert_2.X);
            var c = new Vector3(-facet.vert_3.Y, facet.vert_3.Z, facet.vert_3.X);

            vertSpan[offset + 0] = c;
            vertSpan[offset + 1] = b;
            vertSpan[offset + 2] = a;

            // Recompute normal vector
            var normal = Vector3.Normalize(Vector3.Cross(a - b, c - a));

            normSpan[offset + 0] = normal;
            normSpan[offset + 1] = normal;
            normSpan[offset + 2] = normal;
        }
    }
}