using System.Numerics;
using System.Runtime.InteropServices;

namespace StlVault.IO.Stl
{
    /// <summary>
    /// REAL32[3] – Normal vector
    ///	REAL32[3] – Vertex 1
    ///	REAL32[3] – Vertex 2
    ///	REAL32[3] – Vertex 3
    /// UINT16 – Attribute byte count
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 2)]
    internal readonly struct Facet
    {
        // We'll 100% ignore provided normals, they just cause trouble
        [FieldOffset(00)] private readonly Vector3 normal;
        [FieldOffset(12)] public  readonly Vector3 vert_1;
        [FieldOffset(24)] public  readonly Vector3 vert_2;
        [FieldOffset(36)] public  readonly Vector3 vert_3;
        [FieldOffset(48)] private readonly ushort  _flags;

        public Facet(Vector3 normal, Vector3 vert1, Vector3 vert2, Vector3 vert3)
        {
            this.normal = normal;
            vert_1 = vert1;
            vert_2 = vert2;
            vert_3 = vert3;
            _flags = 0;
        }

        public override string ToString() => $"{normal}: {vert_1}, {vert_2}, {vert_3}";
    }
}