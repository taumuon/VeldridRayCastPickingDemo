using System.Collections.Generic;
using System.Numerics;
using Veldrid;

namespace VeldridRayCast
{
    // Draws a collection of models that share the same render mesh, and differ
    //  only in the model transform.
    // TODO: investigate better ways to do this - single draw call passing all instances model matries
    public class InstancedDrawableMesh : IDrawable, IInstancedSceneObjectContainer
    {
        private DeviceBuffer _vertexBuffer;
        private DeviceBuffer _indexBuffer;

        private uint _countIndices = 0;

        private List<ISceneObject> _instances;

        public bool IsVisible { get; set; } = true;

        // TODO: refactor out duplication with DrawableMesh?
        public InstancedDrawableMesh(Mesh mesh, ResourceFactory resourceFactory, CommandList commandList, int instanceCount)
        {
            _instances = new List<ISceneObject>(instanceCount);

            for (int i = 0; i < instanceCount; i++)
            {
                _instances.Add(new InstancedSceneObject());
            }

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
            commandList.SetVertexBuffer(0, _vertexBuffer);
            commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);

            foreach (var instance in _instances)
            {
                if (instance.IsVisible)
                {
                    Matrix4x4 transform = instance.Transform;
                    commandList.UpdateBuffer(modelBuffer, 0, ref transform);

                    commandList.DrawIndexed(_countIndices, 1, 0, 0, 0);
                }
            }
        }

        // created via DisposeCollectorResourceFactory
        //public void Dispose()
        //{
        //    _vertexBuffer.Dispose();
        //    _indexBuffer.Dispose();
        //}

        public List<ISceneObject> Instances => _instances;

        private class InstancedSceneObject : ISceneObject
        {
            public Matrix4x4 Transform { get; set; }
            public bool IsVisible { get; set; } = true;
        }
    }
}