using System.Numerics;

namespace Physics
{
    public class OBB
    {
        public float Width { get; set; } // X

        public float Height { get; set; } // Y

        public float Length { get; set; } // Z

        public Vector3 NearCorner => new Vector3(-Width / 2.0f, -Height / 2.0f, -Length / 2.0f);

        public Vector3 FarCorner => new Vector3(Width / 2.0f, Height / 2.0f, Length / 2.0f);

        public Matrix4x4 Transform { get; set; }
    }
}
