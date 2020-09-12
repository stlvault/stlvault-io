using System.Numerics;

namespace StlVault.IO
{
    public class MeshStats
    {
        public Bounds Bounds { get; set; }
        public float Volume { get; set; }
        public Vector3 Size => 2 * Bounds.Extends;

        public override string ToString()
        {
            return $"{nameof(Bounds.Center)}: {Bounds.Center}, {nameof(Size)}: {Size}, {nameof(Volume)}: {Volume}";
        }
    }
}