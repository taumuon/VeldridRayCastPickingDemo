namespace VeldridRayCast
{
    public class Mesh
    {
        public VertexPositionColorNormal[] Vertices { get; }
        public ushort[] Indices { get; }

        public Mesh(VertexPositionColorNormal[] vertices, ushort[] indices)
        {
            Vertices = vertices;
            Indices = indices;
        }
    }
}
