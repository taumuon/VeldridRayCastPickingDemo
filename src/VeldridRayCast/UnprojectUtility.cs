using System.Numerics;

namespace VeldridRayCast
{
    public static class UnprojectUtility
    {
        // TODO: code converted from GLM - add copyright info to header
        public static Vector3 Unproject(Vector3 win,
            Matrix4x4 model,
            Matrix4x4 proj,
            Vector4 viewport)
        {
            Matrix4x4 inverse;
            Matrix4x4.Invert(model * proj, out inverse);

            Vector4 tmp = new Vector4(win.X, win.Y, win.Z, 1.0f);
            tmp.X = (tmp.X - viewport.X) / viewport.Z;
            tmp.Y = (tmp.Y - viewport.Y) / viewport.W;
            tmp *= 2;
            tmp -= new Vector4(1.0f, 1.0f, 1.0f, 1.0f);

            Vector4 obj = Vector4.Transform(tmp, inverse);
            obj /= obj.W;

            return new Vector3(obj.X, obj.Y, obj.Z);
        }
    }
}
