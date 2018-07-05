using System.Numerics;
using Veldrid;

namespace VeldridRayCast
{
    public class DrawableMesh : IDrawable, ISceneObject
    {
        private DeviceBuffer _vertexBuffer;
        private DeviceBuffer _indexBuffer;

        private uint _countIndices = 0;

        public Matrix4x4 Transform { get; set; }

        public bool IsVisible { get; set; } = true;

        public DrawableMesh(Mesh mesh, ResourceFactory resourceFactory, CommandList commandList)
        {
            VertexPositionColorNormal[] vertices = mesh.Vertices;
            ushort[] indices = mesh.Indices;

            _countIndices = (uint)indices.Length;

            commandList.Begin();

            _vertexBuffer = resourceFactory.CreateBuffer(new BufferDescription((uint)(VertexPositionColorNormal.SizeInBytes * vertices.Length), BufferUsage.VertexBuffer));
            commandList.UpdateBuffer(_vertexBuffer, 0, vertices);

            _indexBuffer = resourceFactory.CreateBuffer(new BufferDescription(sizeof(ushort) * (uint)indices.Length, BufferUsage.IndexBuffer));
            commandList.UpdateBuffer(_indexBuffer, 0, indices);

            commandList.End();
        }

        public void Draw(CommandList commandList, DeviceBuffer modelBuffer)
        {
            if (!IsVisible) return;

            Matrix4x4 transform = Transform;
            commandList.UpdateBuffer(modelBuffer, 0, ref transform);

            commandList.SetVertexBuffer(0, _vertexBuffer);
            commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);

            commandList.DrawIndexed(_countIndices, 1, 0, 0, 0);
        }

        // created via DisposeCollectorResourceFactory
        //public void Dispose()
        //{
        //    _vertexBuffer.Dispose();
        //    _indexBuffer.Dispose();
        //}
    }
}
