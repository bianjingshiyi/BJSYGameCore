using UnityEngine;

namespace BJSYGameCore
{
    public static class GeometryHelper
    {
        public static float DistanceBetweenPointAndLine(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
        {
            return Mathf.Sin(Vector3.Angle(lineEnd - lineStart, point - lineStart) * Mathf.Deg2Rad) * Vector3.Distance(lineStart, point);
        }
    }
}