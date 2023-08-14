using System.Collections.Generic;
using UnityEngine;

namespace BJSYGameCore.UI
{
    partial class UIManager
    {
        public T createPanel<T>(T prefab) where T : UIPanel
        {
            T instance = Instantiate(prefab, canvas.transform);
            _panelStack.Add(instance);
            instance.Initialize(this);
            return instance;
        }
        public T getPanel<T>() where T : UIObject
        {
            foreach (UIPanel panel in _panelStack)
            {
                if (panel is T)
                    return panel as T;
            }
            return default;
        }

        [SerializeField]
        private List<UIPanel> _panelStack = new List<UIPanel>();
    }
}