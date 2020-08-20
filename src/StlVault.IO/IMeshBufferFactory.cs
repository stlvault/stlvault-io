namespace StlVault.IO
{
    public interface IMeshBufferFactory
    {
        IMeshBuffer RequestBuffer(int vertexCount, bool createNormalBuffer);
    }
}