
using UnityEngine;

namespace BJSYGameCore.SaveSystem
{
    interface IComponentSaver
    {
        ILoadableData save(Component component);
    }
}