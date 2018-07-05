using System.Numerics;

namespace VeldridRayCast
{
    public struct VertexPositionColorNormal
    {
        public static byte SizeInBytes = 36;
        public readonly Vector3 Position;
        public readonly Vector3 Color;
        public readonly Vector3 Normal;

        public VertexPositionColorNormal(Vector3 position, Vector3 color, Vector3 normal)
        {
            Position = position;
            Color = color;
            Normal = normal;
        }
    }
}
