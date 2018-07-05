using System;
using System.Collections.Generic;
using System.Numerics;

namespace Physics
{
    public class World
    {
        public List<OBB> Objects { get; } = new List<OBB>();

        public OBB RayTest(Ray ray)
        {
            OBB obb = null;
            float minIntersectionDistance = float.MaxValue;

            foreach (OBB o in Objects)
            {
                float intersectionDistance;
                if (TestRayOBBIntersection(ray, o, out intersectionDistance))
                {
                    if (intersectionDistance < minIntersectionDistance)
                    {
                        minIntersectionDistance = intersectionDistance;
                        obb = o;
                    }
                }
            }

            return obb;
        }

        // http://www.opengl-tutorial.org/miscellaneous/clicking-on-objects/picking-with-custom-ray-obb-function/
        private static bool TestRayOBBIntersection(Ray ray, OBB obb, out float intersectionDistance)
        {
            Matrix4x4 obbTransform = obb.Transform;
            Vector3 obbPositionWorldSpace = obbTransform.Translation;

            Vector3 delta = obbPositionWorldSpace - ray.Origin;

            float tMin = 0.0f;
            float tMax = float.MaxValue;

            // TODO: check coordinates, compare with glm matrix access (array accessor access column or row?)
            // glm::vec3 xaxis(ModelMatrix[0].x, ModelMatrix[0].y, ModelMatrix[0].z);
            // glm::vec3 yaxis(ModelMatrix[1].x, ModelMatrix[1].y, ModelMatrix[1].z);
            Vector3 xAxis = new Vector3(obbTransform.M11, obbTransform.M21, obbTransform.M31);

            bool intersectX = TestRayOBBIntersectionAxis(ray, ref tMin, ref tMax, xAxis, obb.NearCorner.X, obb.FarCorner.X, delta);

            if (!intersectX || tMax < tMin)
            {
                intersectionDistance = 0.0f;
                return false;
            }

            Vector3 yAxis = new Vector3(obbTransform.M12, obbTransform.M22, obbTransform.M32);

            bool intersectY = TestRayOBBIntersectionAxis(ray, ref tMin, ref tMax, yAxis, obb.NearCorner.Y, obb.FarCorner.Y, delta);

            if (!intersectY || tMax < tMin)
            {
                intersectionDistance = 0.0f;
                return false;
            }

            Vector3 zAxis = new Vector3(obbTransform.M13, obbTransform.M23, obbTransform.M33);

            bool intersectZ = TestRayOBBIntersectionAxis(ray, ref tMin, ref tMax, zAxis, obb.NearCorner.Z, obb.FarCorner.Z, delta);

            if (!intersectZ || tMax < tMin)
            {
                intersectionDistance = 0.0f;
                return false;
            }

            intersectionDistance = tMin;
            return true;
        }

        private static bool TestRayOBBIntersectionAxis(Ray ray,
            ref float tMin,
            ref float tMax,
            Vector3 axis,
            float nearCorner,
            float farCorner,
            Vector3 delta)
        {
            float e = Vector3.Dot(axis, delta);
            float f = Vector3.Dot(ray.Direction, axis);

            if (Math.Abs(f) > 0.001f)
            {
                float t1 = (e + nearCorner) / f;
                float t2 = (e + farCorner) / f;

                if (t1 > t2) { Swap(ref t1, ref t2); }

                tMax = Math.Min(tMax, t2);
                tMin = Math.Max(tMin, t1);
            }
            else if (-e + nearCorner > 0.0f || -e + farCorner < 0.0f)
            {
                return false;
            }

            return true;
        }

        private static void Swap(ref float t1, ref float t2)
        {
            float temp = t1;
            t1 = t2;
            t2 = temp;
        }
    }
}
