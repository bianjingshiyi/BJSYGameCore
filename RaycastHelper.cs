
using UnityEngine;

namespace BJSYGameCore
{
    public static class RaycastHelper
    {
        public static T raycast<T>(Ray ray, float maxDistance, int layerMask) where T : Component
        {
            RaycastHit hit;
            Physics.Raycast(ray, out hit, maxDistance, layerMask);
            if (hit.collider != null)
                return hit.collider.GetComponentInChildren<T>();
            else
                return default;
        }
    }
}