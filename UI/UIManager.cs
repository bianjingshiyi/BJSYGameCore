using UnityEngine;
using BJSYGameCore;

namespace BJSYGameCore.UI
{
    public class UIManager : Manager
    {
        public T getPanel<T>() where T : UIObject
        {
            return GetComponentInChildren<T>(true);
        }
    }
}