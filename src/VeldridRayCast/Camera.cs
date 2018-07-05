using System.Numerics;

namespace VeldridRayCast
{
    public class Camera
    {
        Matrix4x4 _projMatrix;
        Matrix4x4 _viewMatrix;

        public Camera(Matrix4x4 projMatrix, Matrix4x4 viewMatrix)
        {
            _projMatrix = projMatrix;
            _viewMatrix = viewMatrix;
        }

        public Matrix4x4 ProjMatrix => _projMatrix;
        public Matrix4x4 ViewMatrix => _viewMatrix;
    }
}
