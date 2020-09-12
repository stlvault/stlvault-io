namespace StlVault.IO
{
    public class MeshImportSettings
    {
        /// <summary>
        /// Determines if the resulting <see cref="MeshDescriptor.Data"/> will contain normals.
        /// </summary>
        public bool StoreNormals { get; set; } = true;

        /// <summary>
        /// Move the imported vertices so that their AABB is centered on origin (0/0/0).
        /// </summary>
        public bool CenterVertices { get; set; } = false;

        /// <summary>
        /// Compute volume and other stats.
        /// </summary>
        public bool ComputeStats { get; set; } = false;
    }
}