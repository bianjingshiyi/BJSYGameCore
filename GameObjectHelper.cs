using UnityEngine;

namespace BJSYGameCore
{
    public static class GameObjectHelper
    {
        public static void setGameObjectLayer(GameObject gameObject, LayerMask layerMask, bool parentOnly = false)
        {
            gameObject.layer = (int)Mathf.Log(layerMask.value, 2);
            if (!parentOnly)
            {
                for (int i = 0; i < gameObject.transform.childCount; i++)
                {
                    setGameObjectLayer(gameObject.transform.GetChild(i).gameObject, layerMask, parentOnly);
                }
            }
        }
    }
}