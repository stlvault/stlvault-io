using System;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace StlVault.IO
{
    public class Utils
    {
        private static Bounds CalculateBounds(Span<Vector3> vertices)
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

        private static string? ComputeHash(byte[] fileBytes)
        {
            using (var sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(fileBytes);
                return Convert.ToBase64String(hash);
            }
        }

        private static void CenterVertices(Vector3[] vertices, Vector3 correction)
        {
            void MoveVertex(int i) => vertices[i] -= correction;
            Parallel.For(0, vertices.Length, MoveVertex);
        }
    }
}
