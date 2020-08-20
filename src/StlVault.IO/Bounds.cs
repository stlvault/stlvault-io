using System.Numerics;

namespace StlVault.IO
{
    internal struct Bounds
    {
        public Vector3 Center;
        public Vector3 Extends;

        public Bounds(Vector3 center, Vector3 extends)
        {
            Center = center;
            Extends = extends;
        }

        internal static Bounds FromMinMax(Vector3 min, Vector3 max)
        {
            var size = max - min;
            var extends = size / 2;
            var center = min + extends;
            return new Bounds(center, extends);
        }
    }
}