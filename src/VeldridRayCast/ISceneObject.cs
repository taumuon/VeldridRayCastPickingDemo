using System.Numerics;

namespace VeldridRayCast
{
    public interface ISceneObject
    {
        Matrix4x4 Transform { get; set; }

        bool IsVisible { get; set; }
    }
}
