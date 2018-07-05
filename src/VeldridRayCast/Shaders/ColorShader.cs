using ShaderGen;
using System.Numerics;
using static ShaderGen.ShaderBuiltins;

[assembly: ShaderSet("ColorShader", "VeldridRayCast.Shaders.ColorShader.VS", "VeldridRayCast.Shaders.ColorShader.FS")]

namespace VeldridRayCast.Shaders
{
    public class ColorShader
    {
        [ResourceSet(0)]
        public Matrix4x4 Projection;
        [ResourceSet(0)]
        public Matrix4x4 View;
        [ResourceSet(0)]
        public DirectionalLightUniform DirectionalLight;
        [ResourceSet(0)]
        public Texture2DResource ShadowMap;
        [ResourceSet(0)]
        public SamplerResource ShadowMapSampler;


        // Per-object resources (no materials yet)
        // TODO: move diffuse ambient into a per-object uniform
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
            output.Color = input.Color;

            // don't need normal matrix if only uniform scalings
            // http://www.lighthouse3d.com/tutorials/glsl-12-tutorial/the-normal-matrix/

            Vector4 normalModelSpace = Mul(Model, new Vector4(input.Normal, 0.0f));

            output.Normal = normalModelSpace.XYZ();
            output.LightVec = new Vector4(DirectionalLight.Direction, 1.0f);

            Vector4 worldPositionLight = worldPosition;
            Vector4 lightClip = Mul(DirectionalLight.ShadowMatrix, worldPositionLight);
            output.LightCoord = lightClip;

            return output;
        }

        [FragmentShader]
        public Vector4 FS(FragmentInput input)
        {
            // TODO: pass via fragment input
            Vector4 lightColor = new Vector4(1.0f, 1.0f, 1.0f, 0.0f);
            float ambient = 0.2f;
            float diffuse = 1.0f;
            //Vector4 lightColor = new Vector4(DirectionalLight.Color, 1.0f);
            //float ambient = DirectionalLight.AmbientIntensity;
            //float diffuse = DirectionalLight.DiffuseIntensity;

            float shadowBias = 0.0005f;

            Vector3 normalVectorNormalized = Vector3.Normalize(input.Normal);

            Vector3 lightDirectionNormalized = Vector3.Normalize(input.LightVec.XYZ());

            Vector2 shadowCoordsClipped = ClipToTextureCoordinates(input.LightCoord);
            float shadowCoordDepth = input.LightCoord.Z / input.LightCoord.W;

            float shadowSampled = Sample(ShadowMap, ShadowMapSampler, shadowCoordsClipped).X;
            float shadowFactor = shadowCoordDepth - shadowBias < shadowSampled ? 1.0f : 0.0f;

            float diffuseFactor = Vector3.Dot(normalVectorNormalized, lightDirectionNormalized);
            diffuseFactor = Clamp(diffuseFactor, 0.0f, 1.0f) * shadowFactor;

            float brightness = Clamp(ambient + (diffuse * diffuseFactor), 0.0f, 1.0f);

            return new Vector4(input.Color * brightness, 1.0f);

            // To debug shadowmap sampling:
            //float shadowFactorDebugDisplay = shadowSampled;
            //return new Vector4(shadowFactorDebugDisplay, shadowFactorDebugDisplay, shadowFactorDebugDisplay, 1.0f);
        }

        public struct VertexInput
        {
            [PositionSemantic] public Vector3 Position;
            [ColorSemantic] public Vector3 Color;
            [NormalSemantic] public Vector3 Normal;
        }

        public struct FragmentInput
        {
            [SystemPositionSemantic] public Vector4 SystemPosition;
            [ColorSemantic] public Vector3 Color;
            [NormalSemantic] public Vector3 Normal;

            // TODO: why is this texture coordinate semantic
            [TextureCoordinateSemantic] public Vector4 LightVec;

            [TextureCoordinateSemantic] public Vector4 LightCoord;
        }
    }
}
