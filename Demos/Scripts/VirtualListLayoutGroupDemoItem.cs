using System;
using UnityEngine;
using UnityEngine.UI;

namespace BJSYGameCore.Demos
{
    public class VirtualListLayoutGroupDemoItem : MonoBehaviour
    {
        protected void Awake()
        {
            _button.onClick.AddListener(onClickButton);
        }
        void onClickButton()
        {
            onClick?.Invoke(this);
        }
        public event Action<VirtualListLayoutGroupDemoItem> onClick;
        [SerializeField]
        Button _button;
    }
}