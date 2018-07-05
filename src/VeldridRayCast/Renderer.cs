using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Veldrid;
using Veldrid.Utilities;

namespace VeldridRayCast
{
    public class Renderer : IDisposable
    {
        private GraphicsDevice _gd;
        private DisposeCollectorResourceFactory _factory;

        private CommandList _cl;

        private DeviceBuffer _projectionBuffer;
        private DeviceBuffer _viewBuffer;
        private DeviceBuffer _modelBuffer;
        private DeviceBuffer _directionLightBuffer;
        private ResourceSet _projectionViewMatricesLightSet;
        private ResourceSet _perObjectSet;
        private Pipeline _pipeline;

        // TODO: move shadow and non-shadow types into separate classes
        // TODO: can't shadow share the same buffers - it's just the pipeline and sets which are different?!
        private DeviceBuffer _projectionBufferForShadowShader;
        private DeviceBuffer _viewBufferShadow;
        private DeviceBuffer _modelBufferShadow;

        Texture _shadowMap;
        TextureView _shadowMapView;
        Framebuffer _shadowMapFramebuffer;

        private Pipeline _pipelineShadow;
        private ResourceSet _modelMatrixSetShadow;
        private ResourceSet _projectionViewMatricesSetShadow;

        private Camera _camera;

        private List<IDrawable> _drawables = new List<IDrawable>();

        // TODO: expose this somehow, and expose multiple lights
        // TODO: split into two classes, one for direction light,
        //  and split ambient and diffuse onto a per-model(s) material class
        private readonly DirectionalLightUniform _directionalLight;

        private Matrix4x4 _projMatrixLight;
        private Matrix4x4 _viewMatrixLight;

        public Renderer(GraphicsDevice graphicsDevice, Camera camera)
        {
            _gd = graphicsDevice;
            _camera = camera;
            CreateResources();

            // TODO: this is sometimes clipping the shadow, need to calculate scene extents dynamically from all objects in scene
            //  either calculate per loop, or cleverly figure out when objects move
            var nearPlane = 1.0f;
            var farPlane = 25.0f;

            Vector3 lightDirection = new Vector3(-1.0f, 1.0f, 1.0f);
            // TODO: dderive light center from scene extents and light direction
            Vector3 lightCenter = new Vector3(-10.0f, 10.0f, 10.0f);

            // TODO: used for drawing shadow map - refactor out if pulling into separate class.
            _projMatrixLight = Matrix4x4.CreateOrthographic(16.0f, 16.0f, nearPlane, farPlane);
            _viewMatrixLight  = Matrix4x4.CreateLookAt(lightCenter, new Vector3(0.0f, 0.0f, 0.0f), Vector3.UnitY);

            Matrix4x4 shadowMatrix = _viewMatrixLight * _projMatrixLight;

            _directionalLight = new DirectionalLightUniform(
             lightDirection,
             new Vector3(1.0f, 1.0f, 1.0f),
             0.2f,
             1.0f,
             shadowMatrix);
        }

        public ISceneObject Add(Mesh mesh)
        {
            DrawableMesh drawableMesh = new DrawableMesh(mesh, _factory, _cl);
            _gd.SubmitCommands(_cl);
            _gd.WaitForIdle();

            _drawables.Add(drawableMesh);

            return drawableMesh;
        }

        public IInstancedSceneObjectContainer GetInstanceContainer(Mesh mesh, int count)
        {
            InstancedDrawableMesh instancedDrawableMesh = new InstancedDrawableMesh(mesh, _factory, _cl, count);
            _gd.SubmitCommands(_cl);
            _gd.WaitForIdle();

            _drawables.Add(instancedDrawableMesh);

            return instancedDrawableMesh;
        }

        private void CreateResources()
        {
            _factory = new DisposeCollectorResourceFactory(_gd.ResourceFactory);

            _cl = _factory.CreateCommandList();

            _cl.Begin();
            _projectionBuffer = _factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            _viewBuffer = _factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            _modelBuffer = _factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));

            _projectionBufferForShadowShader = _factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            _viewBufferShadow = _factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            _modelBufferShadow = _factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));

            // TODO: no idea why this buffer requires 48 bytes instead of 32 bytes, padding?
            //_directionLightBuffer = _factory.CreateBuffer(new BufferDescription(48, BufferUsage.UniformBuffer));
            // addition of shadowmatrix, now requires (48+64=) 112 bytes
            _directionLightBuffer = _factory.CreateBuffer(new BufferDescription(112, BufferUsage.UniformBuffer));

            _cl.End();
            _gd.SubmitCommands(_cl);
            _gd.WaitForIdle();

            ShaderSetDescription shaderSet = new ShaderSetDescription(
                new[]
                {
                    new VertexLayoutDescription(
                        new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float3),
                        new VertexElementDescription("Color", VertexElementSemantic.Color, VertexElementFormat.Float3),
                        new VertexElementDescription("Normal", VertexElementSemantic.Normal, VertexElementFormat.Float3))
                },
                new[]
                {
                    LoadShader(_factory, "ColorShader", ShaderStages.Vertex, "VS"),
                    LoadShader(_factory, "ColorShader", ShaderStages.Fragment, "FS")
                });

            ShaderSetDescription shaderSetShadow = new ShaderSetDescription(
                new[]
                {
                    new VertexLayoutDescription(
                        new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float3),
                        new VertexElementDescription("Color", VertexElementSemantic.Color, VertexElementFormat.Float3),
                        new VertexElementDescription("Normal", VertexElementSemantic.Normal, VertexElementFormat.Float3))
                },
                new[]
                {
                    LoadShader(_factory, "ShadowShader", ShaderStages.Vertex, "VS"),
                    LoadShader(_factory, "ShadowShader", ShaderStages.Fragment, "FS")
                });

            ResourceLayout projectionViewMatricesLightLayout = _factory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("Projection", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                    new ResourceLayoutElementDescription("View", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                    new ResourceLayoutElementDescription("DirectionalLight", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                    new ResourceLayoutElementDescription("ShadowMap", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("ShadowMapSampler", ResourceKind.Sampler, ShaderStages.Fragment)
                    ));

            ResourceLayout perObjectLayout = _factory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("Model", ResourceKind.UniformBuffer, ShaderStages.Vertex)
                    ));

            ResourceLayout projectionViewMatricesLayoutShadow = _factory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("Projection", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                    new ResourceLayoutElementDescription("View", ResourceKind.UniformBuffer, ShaderStages.Vertex)
                    ));

            ResourceLayout modelLayoutShadow = _factory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("Model", ResourceKind.UniformBuffer, ShaderStages.Vertex)
                    ));

            _pipeline = _factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerStateDescription.Default,
                PrimitiveTopology.TriangleList,
                shaderSet,
                new[] { projectionViewMatricesLightLayout, perObjectLayout },
                _gd.SwapchainFramebuffer.OutputDescription));

            TextureDescription desc = TextureDescription.Texture2D(2048, 2048, 1, 1, PixelFormat.D32_Float_S8_UInt, TextureUsage.DepthStencil | TextureUsage.Sampled);
            _shadowMap = _factory.CreateTexture(desc);
            _shadowMap.Name = "Shadow Map";
            _shadowMapView = _factory.CreateTextureView(_shadowMap);
            _shadowMapFramebuffer = _factory.CreateFramebuffer(new FramebufferDescription(
                new FramebufferAttachmentDescription(_shadowMap, 0), Array.Empty<FramebufferAttachmentDescription>()));


            _pipelineShadow = _factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
                BlendStateDescription.Empty,
                DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerStateDescription.Default,
                PrimitiveTopology.TriangleList,
                shaderSetShadow,
                new[] { projectionViewMatricesLayoutShadow, modelLayoutShadow },
                _shadowMapFramebuffer.OutputDescription));

            _projectionViewMatricesLightSet = _factory.CreateResourceSet(new ResourceSetDescription(
                projectionViewMatricesLightLayout,
                _projectionBuffer,
                _viewBuffer,
                _directionLightBuffer,
                _shadowMapView,
                _gd.PointSampler));

            _perObjectSet = _factory.CreateResourceSet(new ResourceSetDescription(
                perObjectLayout,
                _modelBuffer));

            _projectionViewMatricesSetShadow = _factory.CreateResourceSet(new ResourceSetDescription(
                projectionViewMatricesLayoutShadow,
                _projectionBufferForShadowShader,
                _viewBufferShadow));

            _modelMatrixSetShadow = _factory.CreateResourceSet(new ResourceSetDescription(
                modelLayoutShadow,
                _modelBufferShadow));
        }

        private void DrawScene()
        {
            _cl.SetFramebuffer(_gd.SwapchainFramebuffer);
            _cl.SetFullViewports();
            _cl.ClearColorTarget(0, RgbaFloat.Black);
            _cl.ClearDepthStencil(1f);

            _cl.SetPipeline(_pipeline);

            //_cl.UpdateBuffer(_projectionBuffer, 0, _projMatrixLight);
            //_cl.UpdateBuffer(_viewBuffer, 0, _viewMatrixLight);
            _cl.UpdateBuffer(_projectionBuffer, 0, _camera.ProjMatrix);
            _cl.UpdateBuffer(_viewBuffer, 0, _camera.ViewMatrix);

            _cl.UpdateBuffer(_directionLightBuffer, 0, _directionalLight);

            _cl.SetGraphicsResourceSet(0, _projectionViewMatricesLightSet);
            _cl.SetGraphicsResourceSet(1, _perObjectSet);

            foreach (var drawableMesh in _drawables)
            {
                drawableMesh.Draw(_cl, _modelBuffer);
            }
        }

        private void DrawToShadowMap()
        {
            _cl.SetFramebuffer(_shadowMapFramebuffer);
            _cl.SetFullViewports();
            _cl.ClearDepthStencil(01f);

            _cl.UpdateBuffer(_projectionBufferForShadowShader, 0, _projMatrixLight);
            _cl.UpdateBuffer(_viewBufferShadow, 0, _viewMatrixLight);

            _cl.SetPipeline(_pipelineShadow);

            _cl.SetGraphicsResourceSet(0, _projectionViewMatricesSetShadow);
            _cl.SetGraphicsResourceSet(1, _modelMatrixSetShadow);

            foreach (var drawableMesh in _drawables)
            {
                drawableMesh.Draw(_cl, _modelBufferShadow);
            }
        }

        public void Draw()
        {
            _cl.Begin();

            DrawToShadowMap();
            DrawScene();

            _cl.End();
            _gd.SubmitCommands(_cl);
            _gd.SwapBuffers();
        }

        public static Shader LoadShader(ResourceFactory factory, string set, ShaderStages stage, string entryPoint)
        {
            string path = Path.Combine(
                AppContext.BaseDirectory,
                "Shaders",
                $"{set}-{stage.ToString().ToLower()}.{GetExtension(factory.BackendType)}");
            return factory.CreateShader(new ShaderDescription(stage, File.ReadAllBytes(path), entryPoint));
        }

        private static string GetExtension(GraphicsBackend backendType)
        {
            return (backendType == GraphicsBackend.Direct3D11)
                ? "hlsl.bytes"
                : (backendType == GraphicsBackend.Vulkan)
                    ? "450.glsl.spv"
                    : (backendType == GraphicsBackend.Metal)
                        ? "metallib"
                        : "330.glsl";
        }

        public void ResizeMainWindow(uint width, uint height)
        {
            _gd.ResizeMainWindow(width, height);
        }

        public void Dispose()
        {
            // Would want to expose this - let the caller wait for idle before calling dispose?
            _gd.WaitForIdle();

            _factory.DisposeCollector.DisposeAll();
            _gd.Dispose();
        }
    }
}
