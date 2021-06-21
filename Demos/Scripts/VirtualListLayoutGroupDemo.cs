using BJSYGameCore.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BJSYGameCore.Demos
{
    /// <summary>
    /// 这个脚本演示了该如何使用VirtualListLayoutGroup。
    /// </summary>
    public class VirtualListLayoutGroupDemo : MonoBehaviour
    {
        protected void Awake()
        {
            _countField.onEndEdit.AddListener(onEndEditCount);
            //首先你需要注册onEnableItem事件。
            //由于VirtualList中的单元格物体是动态生成的，所以你无法获取那些没有被显示的单元格对应的物体。
            //注册这个事件，你就可以在这些单元格显示的时候对它们进行初始化。
            _virtualList.onEnableItem += onEnableItem;
            _virtualList.onUpdateItem += onUpdateItem;
            _virtualList.onDisableItem += onDisableItem;
        }
        protected void Start()
        {
            //设置virtualList的count来指定你需要显示多少个单元格。
            //只有目前能被看到的单元格会被创建，并触发onEnableItem事件。
            _virtualList.setCount(100);
        }
        private void onEnableItem(int index, RectTransform transform)
        {
            //当单元格被创建或者重新被显示的时候会触发这个事件。
            //你可以在事件回调中对创建或者重新显示的单元格进行初始化。
            transform.GetComponentInChildren<VirtualListLayoutGroupDemoItem>().onClick += onItemClick;
        }
        private void onUpdateItem(int index, RectTransform transform)
        {
            //这个事件会在单元格需要更新的时候被调用，比如当你重新设置数量的时候会对所有当前显示的单元格调用一遍。
            //在这个回调里执行一些更新逻辑来保持单元格内容最新，但是不要在这里执行初始化，这样可能会引发重复初始化。
            transform.GetComponentInChildren<Text>().text = DateTime.Now + " - " + index.ToString();
        }
        private void onDisableItem(int index, RectTransform transform)
        {
            //如果你注册了事件，不要忘记在单元格被隐藏的时候注销它。
            transform.GetComponentInChildren<VirtualListLayoutGroupDemoItem>().onClick -= onItemClick;
        }
        void onItemClick(VirtualListLayoutGroupDemoItem item)
        {
            Debug.Log(_virtualList.getItemIndex(item.transform as RectTransform));
        }
        void onEndEditCount(string value)
        {
            if (int.TryParse(value, out int count))
            {
                if (count < 0)
                    count = 0;
                _virtualList.setCount(count);
            }
        }
        [SerializeField]
        InputField _countField;
        [SerializeField]
        VirtualListLayoutGroup _virtualList;
    }
}