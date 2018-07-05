using System.Numerics;

namespace VeldridRayCast
{
    public static class CubeMeshFactory
    {
        public static Mesh GetMesh()
        {
            var orange = new Vector3(1.0f, 0.5f, 0.0f);
            var green = new Vector3(0.0f, 1.0f, 0.0f);
            var blue = new Vector3(0.0f, 0.0f, 1.0f);
            var yellow = new Vector3(1.0f, 1.0f, 0.0f);
            var purple = new Vector3(1.0f, 0.0f, 1.0f);
            var cyan = new Vector3(0.0f, 1.0f, 1.0f);

            return new Mesh(GetCubeVertices(topColor: orange,
                bottomColor: green,
                leftColor: blue,
                rightColor: yellow,
                backColor: purple,
                frontColor: cyan),
                GetCubeIndices());
        }

        public static Mesh GetMesh(Vector3 color)
        {
            return new Mesh(GetCubeVertices(color, color, color, color, color, color), GetCubeIndices());
        }

        private static VertexPositionColorNormal[] GetCubeVertices(Vector3 topColor,
            Vector3 bottomColor,
            Vector3 leftColor,
            Vector3 rightColor,
            Vector3 backColor,
            Vector3 frontColor)
        {
            Vector3 topNormal = new Vector3(0.0f, 1.0f, 0.0f);
            Vector3 bottomNormal = new Vector3(0.0f, -1.0f, 0.0f);
            Vector3 leftNormal = new Vector3(-1.0f, 0.0f, 0.0f);
            Vector3 rightNormal = new Vector3(1.0f, 0.0f, 0.0f);
            Vector3 backNormal = new Vector3(0.0f, 0.0f, -1.0f);
            Vector3 frontNormal = new Vector3(0.0f, 0.0f, 1.0f);

            VertexPositionColorNormal[] vertices = new VertexPositionColorNormal[]
            {
                // Top
                new VertexPositionColorNormal(new Vector3(-0.5f, +0.5f, -0.5f), topColor, topNormal),
                new VertexPositionColorNormal(new Vector3(+0.5f, +0.5f, -0.5f), topColor, topNormal),
                new VertexPositionColorNormal(new Vector3(+0.5f, +0.5f, +0.5f), topColor, topNormal),
                new VertexPositionColorNormal(new Vector3(-0.5f, +0.5f, +0.5f), topColor, topNormal),
                // Bottom                                                             
                new VertexPositionColorNormal(new Vector3(-0.5f,-0.5f, +0.5f),  bottomColor, bottomNormal),
                new VertexPositionColorNormal(new Vector3(+0.5f,-0.5f, +0.5f),  bottomColor, bottomNormal),
                new VertexPositionColorNormal(new Vector3(+0.5f,-0.5f, -0.5f),  bottomColor, bottomNormal),
                new VertexPositionColorNormal(new Vector3(-0.5f,-0.5f, -0.5f),  bottomColor, bottomNormal),
                // Left                                                               
                new VertexPositionColorNormal(new Vector3(-0.5f, +0.5f, -0.5f), leftColor, leftNormal),
                new VertexPositionColorNormal(new Vector3(-0.5f, +0.5f, +0.5f), leftColor, leftNormal),
                new VertexPositionColorNormal(new Vector3(-0.5f, -0.5f, +0.5f), leftColor, leftNormal),
                new VertexPositionColorNormal(new Vector3(-0.5f, -0.5f, -0.5f), leftColor, leftNormal),
                // Right                                                              
                new VertexPositionColorNormal(new Vector3(+0.5f, +0.5f, +0.5f), rightColor, rightNormal),
                new VertexPositionColorNormal(new Vector3(+0.5f, +0.5f, -0.5f), rightColor, rightNormal),
                new VertexPositionColorNormal(new Vector3(+0.5f, -0.5f, -0.5f), rightColor, rightNormal),
                new VertexPositionColorNormal(new Vector3(+0.5f, -0.5f, +0.5f), rightColor, rightNormal),
                // Back                                                               
                new VertexPositionColorNormal(new Vector3(+0.5f, +0.5f, -0.5f), backColor, backNormal),
                new VertexPositionColorNormal(new Vector3(-0.5f, +0.5f, -0.5f), backColor, backNormal),
                new VertexPositionColorNormal(new Vector3(-0.5f, -0.5f, -0.5f), backColor, backNormal),
                new VertexPositionColorNormal(new Vector3(+0.5f, -0.5f, -0.5f), backColor, backNormal),
                // Front                                                              
                new VertexPositionColorNormal(new Vector3(-0.5f, +0.5f, +0.5f), frontColor, frontNormal),
                new VertexPositionColorNormal(new Vector3(+0.5f, +0.5f, +0.5f), frontColor, frontNormal),
                new VertexPositionColorNormal(new Vector3(+0.5f, -0.5f, +0.5f), frontColor, frontNormal),
                new VertexPositionColorNormal(new Vector3(-0.5f, -0.5f, +0.5f), frontColor, frontNormal),
            };

            return vertices;
        }

        private static ushort[] GetCubeIndices()
        {
            ushort[] indices =
            {
                0,1,2, 0,2,3,
                4,5,6, 4,6,7,
                8,9,10, 8,10,11,
                12,13,14, 12,14,15,
                16,17,18, 16,18,19,
                20,21,22, 20,22,23,
            };

            return indices;
        }
    }
}
