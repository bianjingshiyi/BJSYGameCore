using System;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace TBSGameCore
{
    [Serializable]
    public class ObjectReference
    {
        public int id;
        public ObjectReference(int id)
        {
            this.id = id;
        }
        public static ObjectReference convert<T>(T obj,IObjectKeeper<T> keeper) where T : UnityEngine.Object
        {
            return new ObjectReference(keeper.getIdOfObject(obj));
        }
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