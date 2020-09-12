using System;
using System.IO;
using System.Numerics;
using StlVault.IO.Stl;

namespace StlVault.IO
{
    public class MeshImporter
    {
        private readonly IMeshBufferFactory _factory;

        public MeshImporter() : this(new ArrayPoolBufferFactory()) { }
        public MeshImporter(IMeshBufferFactory factory) => _factory = factory;

        public MeshDescriptor ImportFromFile(string filePath, MeshImportSettings? settings = null)
        {
            settings ??= new MeshImportSettings();

            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            switch (extension)
            {
                case ".stl":
                    var buffer = StlImporter.ImportMesh(filePath, _factory, settings.StoreNormals);
                    return Wrap(buffer, settings);
                default:
                    throw new NotSupportedException("3D models in the supplied format are currently not supported.");
            }
        }

        private static MeshDescriptor Wrap(IMeshBuffer buffer, MeshImportSettings settings)
        {
            var vertices = buffer.VertexData;
            var bounds = CalculateBounds(vertices);

            if (settings.CenterVertices)
            {
                CenterVertices(vertices, bounds.Center);
                bounds = new Bounds(Vector3.Zero, bounds.Extends);
            }

            var stats = settings.ComputeStats
                ? new MeshStats
                {
                    Bounds = bounds,
                    Volume = CalculateVolume(vertices)
                }
                : null;

            return new MeshDescriptor(buffer, stats);
        }

        private static float CalculateVolume(in Span<Vector3> vertices)
        {
            var volume = 0f;
            for (var i = 0; i < vertices.Length; i += 3)
            {
                var a = vertices[i + 0];
                var b = vertices[i + 1];
                var c = vertices[i + 2];

                volume += Vector3.Dot(Vector3.Cross(a, b), c) / 6f;
            }

            return Math.Abs(volume) / 1000f;
        }

        private static void CenterVertices(in Span<Vector3> vertices, Vector3 correction)
        {
            for (var i = 0; i < vertices.Length; i++)
            {
                vertices[i] -= correction;
            }
        }

        // private static string ComputeHash(byte[] fileBytes)
        // {
        //     using var sha1 = new SHA1Managed();
        //     var hash = sha1.ComputeHash(fileBytes);
        //     return Convert.ToBase64String(hash);
        // }

        private static Bounds CalculateBounds(in Span<Vector3> vertices)
        {
            var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            for (var i = 0; i < vertices.Length; i++)
            {
                ref var current = ref vertices[i];
                min = Vector3.Min(current, min);
                max = Vector3.Max(current, max);
            }

            return Bounds.FromMinMax(min, max);
        }
    }
}