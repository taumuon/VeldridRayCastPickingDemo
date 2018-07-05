using System.Numerics;
using Veldrid;

namespace VeldridRayCast
{
    public interface IDrawable
    {
        void Draw(CommandList commandList, DeviceBuffer modelBuffer);
    }
}
