using BJSYGameCore.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BJSYGameCore.Demos
{
    public class VirtualListLayoutGroupDemo : MonoBehaviour
    {
        protected void Awake()
        {
            _virtualList.onEnableItem += onEnableItem;
        }
        protected void Start()
        {
            _virtualList.setCount(100);
        }
        private void onEnableItem(int index, RectTransform item)
        {
            item.GetComponentInChildren<Text>().text = index.ToString();
        }
        [SerializeField]
        VirtualListLayoutGroup _virtualList;
    }
}