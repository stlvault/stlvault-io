using System.Numerics;
using Xunit;

namespace StlVault.IO.Tests
{
    public class ArrayPoolBufferFactoryTests
    {
        [Fact]
        public void CanRentWithNormals()
        {
            var factory = new ArrayPoolBufferFactory();
            using var buffer = factory.RequestBuffer(3, createNormalBuffer: true);

            Assert.Equal(3, buffer.VertexData.Length);
            Assert.Equal(3, buffer.NormalData.Length);
        }

        [Fact]
        public void CanRentWithoutNormals()
        {
            var factory = new ArrayPoolBufferFactory();
            using var buffer = factory.RequestBuffer(3, createNormalBuffer: false);

            Assert.Equal(3, buffer.VertexData.Length);
            Assert.Equal(0, buffer.NormalData.Length);
        }

        [Fact]
        public void DataInIsDataOut()
        {
            var factory = new ArrayPoolBufferFactory();
            using var buffer = factory.RequestBuffer(2, createNormalBuffer: true);

            var vertices = buffer.VertexData;
            var normals = buffer.NormalData;

            vertices[0] = Vector3.Zero;
            vertices[1] = Vector3.One;
            normals[0] = Vector3.UnitX;
            normals[1] = Vector3.UnitY;

            Assert.Equal(Vector3.Zero, buffer.VertexData[0]);
            Assert.Equal(Vector3.One, buffer.VertexData[1]);
            Assert.Equal(Vector3.UnitX, buffer.NormalData[0]);
            Assert.Equal(Vector3.UnitY, buffer.NormalData[1]);
        }

        [Fact]
        public void ShinkingShrinksTheBuffer()
        {
            var factory = new ArrayPoolBufferFactory();
            using var buffer = factory.RequestBuffer(3, createNormalBuffer: true);
            buffer.Shrink(1);

            Assert.Equal(1, buffer.VertexData.Length);
            Assert.Equal(1, buffer.NormalData.Length);
        }

        [Fact]
        public void ShinkingSlicesTheRightData()
        {
            var factory = new ArrayPoolBufferFactory();
            using var buffer = factory.RequestBuffer(2, createNormalBuffer: true);

            var vertices = buffer.VertexData;
            var normals = buffer.NormalData;

            vertices[0] = Vector3.Zero;
            vertices[1] = Vector3.One;
            normals[0] = Vector3.UnitX;
            normals[1] = Vector3.UnitY;

            buffer.Shrink(1);

            Assert.Equal(Vector3.Zero, buffer.VertexData[0]);
            Assert.Equal(Vector3.UnitX, buffer.NormalData[0]);
        }
    }
}
