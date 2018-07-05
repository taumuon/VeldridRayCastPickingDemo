using System.Collections.Generic;

namespace VeldridRayCast
{
    public interface IInstancedSceneObjectContainer
    {
        List<ISceneObject> Instances { get; }
    }
}
