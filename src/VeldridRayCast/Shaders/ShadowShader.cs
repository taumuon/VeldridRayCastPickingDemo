using ShaderGen;
using System.Numerics;
using static ShaderGen.ShaderBuiltins;

[assembly: ShaderSet("ShadowShader", "VeldridRayCast.Shaders.ShadowShader.VS", "VeldridRayCast.Shaders.ShadowShader.FS")]

namespace VeldridRayCast.Shaders
{
    public class ShadowShader
    {
        [ResourceSet(0)]
        public Matrix4x4 Projection;
        [ResourceSet(0)]
        public Matrix4x4 View;

        [ResourceSet(1)]
        public Matrix4x4 Model;

        [VertexShader]
        public FragmentInput VS(VertexInput input)
        {
            FragmentInput output;
            Vector4 worldPosition = Mul(Model, new Vector4(input.Position, 1));
            Vector4 viewPosition = Mul(View, worldPosition);
            Vector4 clipPosition = Mul(Projection, viewPosition);
            output.SystemPosition = clipPosition;

            return output;
        }

        [FragmentShader]
        public Vector4 FS(FragmentInput input)
        {
            return new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        }
        // public void FS(FragmentInput fragmentInput) { }

        public struct VertexInput
        {
            [PositionSemantic] public Vector3 Position;
        }

        public struct FragmentInput
        {
            [SystemPositionSemantic] public Vector4 SystemPosition;
        }
    }
}
