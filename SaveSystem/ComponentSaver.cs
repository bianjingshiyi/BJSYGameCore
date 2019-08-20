
using UnityEngine;

namespace BJSYGameCore.SaveSystem
{
    public abstract class ComponentSaver<T> : IComponentSaver where T : Component
    {
        public abstract ILoadableData save(T component);
        ILoadableData IComponentSaver.save(Component component)
        {
            return save(component as T);
        }
    }
}