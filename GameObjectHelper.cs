using UnityEngine;

namespace BJSYGameCore
{
    public static class GameObjectHelper
    {
        public static void setGameObjectLayer(GameObject gameObject, LayerMask layerMask)
        {
            gameObject.layer = (int)Mathf.Log(layerMask.value, 2);
        }
    }
}