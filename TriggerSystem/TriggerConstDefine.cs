using UnityEngine;

namespace TBSGameCore.TriggerSystem
{
    public abstract class TriggerConstDefine : TriggerExprDefine
    {
    }
    public abstract class TriggerConstDefine<T> : TriggerConstDefine where T : ConstExprComponent
    {
        public override TriggerExprComponent attachTo(GameObject go)
        {
            return go.AddComponent<T>();
        }
        public override TriggerExprComponent createInstance(Transform parent)
        {
            T instance = new GameObject(name).AddComponent<T>();
            instance.transform.parent = parent;
            return instance;
        }
    }
}
