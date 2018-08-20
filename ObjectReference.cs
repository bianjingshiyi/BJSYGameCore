using System;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace TBSGameCore
{
    [Serializable]
    public class ObjectReference
    {
        public int id;
        public T getValue<T>(IObjectKeeper<T> keeper) where T : UnityEngine.Object
        {
            if (keeper != null)
                return keeper.getObjectById(id);
            else
                return null;
        }
        public T getValue<T>(Scene scene) where T : UnityEngine.Object
        {
            Type keeperType = typeof(IObjectKeeper<>).MakeGenericType(typeof(T));
            IObjectKeeper<T> keeper = scene.findInstance(keeperType) as IObjectKeeper<T>;
            if (keeper != null)
                return keeper.getObjectById(id);
            else
                return null;
        }
    }
}