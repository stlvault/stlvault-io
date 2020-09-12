using System;

namespace StlVault.IO
{
    public sealed class MeshDescriptor : IDisposable
    {
        public MeshDescriptor(IMeshBuffer data, MeshStats? stats)
        {
            Data = data;
            Stats = stats;
        }

        public IMeshBuffer Data { get; }
        public MeshStats? Stats { get; }

        public void Dispose()
        {
            Data.Dispose();
        }
    }
}