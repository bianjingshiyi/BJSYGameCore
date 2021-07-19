using System;
using UnityEngine;
using UnityEngine.UI;

namespace BJSYGameCore.Demos
{
    public class UIHelperUpdateListDemoItem : MonoBehaviour
    {
        protected void Awake()
        {
            _button.onClick.AddListener(onClickCallback);
        }
        void onClickCallback()
        {
            onClick?.Invoke(transform as RectTransform);
        }
        public event Action<RectTransform> onClick;
        [SerializeField]
        Button _button;
    }
}