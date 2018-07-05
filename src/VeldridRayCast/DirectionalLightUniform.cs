using System.Numerics;
using System.Runtime.InteropServices;

namespace VeldridRayCast
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DirectionalLightUniform
    {
        // TODO: ShadowMap sampling is broken if this matrix is moved to last field in struct
        //  - alignment issue? need to pad out?
        public readonly Matrix4x4 ShadowMatrix;

        // Light direction world space
        public readonly Vector3 Direction;
        //public float Padding1;
        public readonly Vector3 Color;
        //public float Padding2;

        // TODO: the following are model properties, should
        //  be split into a separate uniform to pass per model
        //  (or per group of models sharing same material)
        public readonly float AmbientIntensity;
        public readonly float DiffuseIntensity;

        public DirectionalLightUniform(Vector3 direction, Vector3 color, float ambientIntensity, float diffuseIntensity, Matrix4x4 shadowMatrix)
        {
            Direction = direction;
            Color = color;
            AmbientIntensity = ambientIntensity;
            DiffuseIntensity = diffuseIntensity;
            ShadowMatrix = shadowMatrix;

            //Padding1 = 0.0f;
            //Padding2 = 0.0f;
        }
    }
}
