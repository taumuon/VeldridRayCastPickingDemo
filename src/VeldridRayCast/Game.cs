using Physics;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace VeldridRayCast
{
    public class Game
    {
        private float _ticks;

        protected readonly Sdl2Window _window;
        private bool _windowResized;

        private List<ISceneObject> _cubes;
        private Dictionary<OBB, ISceneObject> _physicsToSceneObjectMap;

        private World _collisionWorld;

        private Camera _camera;

        private Renderer _renderer;

        private ISceneObject _selection;

        private ISceneObject _yellowRotatable;
        private ISceneObject _greenRotatable;
        private ISceneObject _blueRotatable;

        private static readonly Matrix4x4 _yellowBaseTransform = Matrix4x4.CreateTranslation(-2.0f, 2.0f, 0.0f);
        private static readonly Matrix4x4 _greenBaseTransform = Matrix4x4.CreateTranslation(0.0f, 2.0f, 0.0f);
        private static readonly Matrix4x4 _blueBaseTransform = Matrix4x4.CreateTranslation(2.0f, 2.0f, 0.0f);

        private float _rotation = 0.0f;

        // TODO: temporary, just to test lighting
        private List<ISceneObject> _rotationCubes = new List<ISceneObject>();

        public Game()
        {
            _collisionWorld = new World();
            _physicsToSceneObjectMap = new Dictionary<OBB, ISceneObject>();

            WindowCreateInfo wci = new WindowCreateInfo
            {
                X = 100,
                Y = 100,
                WindowWidth = 960,
                WindowHeight = 540,
                WindowTitle = GetTitle(),
            };
            _window = VeldridStartup.CreateWindow(ref wci);
            _window.Resized += () =>
            {
                _windowResized = true;
                OnWindowResized();
            };

            Matrix4x4 projMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
                1.0f,
                (float)_window.Width / _window.Height,
                0.5f,
                100f);
            Matrix4x4 viewMatrix = Matrix4x4.CreateLookAt(new Vector3(-1.0f, 5.0f, 8.5f), new Vector3(0.0f, -1.0f, -1.5f), Vector3.UnitY);
            _camera = new Camera(projMatrix, viewMatrix);

            GraphicsDeviceOptions options = new GraphicsDeviceOptions(false, PixelFormat.R16_UNorm, true);
#if DEBUG
            options.Debug = true;
#endif
            GraphicsDevice gd = VeldridStartup.CreateGraphicsDevice(_window, options, GraphicsBackend.Direct3D11);

            _renderer = new Renderer(gd, _camera);

            var instancedDrawable = _renderer.GetInstanceContainer(CubeMeshFactory.GetMesh(), 121);
            _cubes = instancedDrawable.Instances;

            for (int x = 0; x < 11; x++)
            {
                for (int z = 0; z < 11; z++)
                {
                    Vector3 pos = new Vector3(x - 5, 0, z - 5);
                    var transform = Matrix4x4.CreateTranslation(pos - new Vector3(0.5f, 0.5f, 0.5f));
                    ISceneObject renderCube = _cubes[x * 11 + z];
                    renderCube.Transform = transform;

                    OBB physicsCube = new OBB { Height = 1.0f, Width = 1.0f, Length = 1.0f, Transform = transform };

                    _collisionWorld.Objects.Add(physicsCube);

                    _physicsToSceneObjectMap.Add(physicsCube, renderCube);
                }
            }

            _selection = _renderer.Add(CubeMeshFactory.GetMesh(new Vector3(1.0f, 0.0f, 0.0f)));
            _selection.IsVisible = false;

            _yellowRotatable = _renderer.Add(CubeMeshFactory.GetMesh(new Vector3(1.0f, 1.0f, 0.0f)));
            _greenRotatable = _renderer.Add(CubeMeshFactory.GetMesh(new Vector3(0.0f, 1.0f, 0.0f)));
            _blueRotatable = _renderer.Add(CubeMeshFactory.GetMesh(new Vector3(0.0f, 0.0f, 1.0f)));

            _yellowRotatable.Transform = _yellowBaseTransform;
            _greenRotatable.Transform = _greenBaseTransform;
            _blueRotatable.Transform = _blueBaseTransform;
        }

        private void Draw(float deltaSeconds)
        {
            _ticks += deltaSeconds * 1000f;

            _rotation += 0.01f;

            _yellowRotatable.Transform = Matrix4x4.CreateFromAxisAngle(new Vector3(1.0f, 0.0f, 0.0f), _rotation) * _yellowBaseTransform; ;
            _greenRotatable.Transform = Matrix4x4.CreateFromAxisAngle(new Vector3(0.0f, 1.0f, 0.0f), _rotation) * _greenBaseTransform;
            _blueRotatable.Transform = Matrix4x4.CreateFromAxisAngle(new Vector3(0.0f, 0.0f, 1.0f), _rotation) * _blueBaseTransform;

            _renderer.Draw();
        }

        public void RunGameLoop()
        {
            Stopwatch sw = Stopwatch.StartNew();
            double previousElapsed = sw.Elapsed.TotalSeconds;

            while (_window.Exists)
            {
                double newElapsed = sw.Elapsed.TotalSeconds;
                float deltaSeconds = (float)(newElapsed - previousElapsed);

                InputSnapshot inputSnapshot = _window.PumpEvents();
                InputTracker.UpdateFrameInput(inputSnapshot);

                if (_window.Exists)
                {
                    previousElapsed = newElapsed;
                    if (_windowResized)
                    {
                        _renderer.ResizeMainWindow((uint)_window.Width, (uint)_window.Height);
                        HandleWindowResize();
                    }

                    ProcessInput();
                    Draw(deltaSeconds);
                }
            }

            _renderer.Dispose();
        }

        private void ProcessInput()
        {
            //if (InputTracker.GetMouseButtonDown(MouseButton.Left))
            if (!InputTracker.GetMouseButton(MouseButton.Left)) { return; }

            Vector3 origin;
            Vector3 direction;
            ScreenPosToWorldRay((int)InputTracker.MousePosition.X, (int)InputTracker.MousePosition.Y, out origin, out direction);

            Ray ray = new Ray { Origin = origin, Direction = direction };
            OBB obb = _collisionWorld.RayTest(ray);
            if (obb == null) { return; }

            ISceneObject selectedCube;
            if (_physicsToSceneObjectMap.TryGetValue(obb, out selectedCube))
            {
                _selection.Transform = Matrix4x4.CreateScale(1.05f) * selectedCube.Transform;
            }
            _selection.IsVisible = selectedCube != null;
        }

        private string GetTitle() => GetType().Name;

        private void OnWindowResized()
        {
            // TODO: recreate projection matrix. Need to do anything else?
        }

        private void HandleWindowResize() { }

        private void ScreenPosToWorldRay(int mouseX, int mouseY,
            out Vector3 origin,
            out Vector3 direction)
        {
            mouseY = _window.Height - mouseY;

            Vector4 viewport = new Vector4(0.0f, 0.0f, _window.Width, _window.Height);
            Vector3 rayStart = UnprojectUtility.Unproject(new Vector3(mouseX, mouseY, 0.0f), _camera.ViewMatrix, _camera.ProjMatrix, viewport);
            Vector3 rayEnd = UnprojectUtility.Unproject(new Vector3(mouseX, mouseY, 1.0f), _camera.ViewMatrix, _camera.ProjMatrix, viewport);

            origin = rayStart;
            direction = Vector3.Normalize(rayEnd - rayStart);
        }
    }
}
