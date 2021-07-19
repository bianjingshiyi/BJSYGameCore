using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
namespace BJSYGameCore.UI
{
    public static class UIHelper
    {
        /// <summary>
        /// 刷新列表，在根节点下创建或销毁列表项直到指定数量，然后调用回调来进行更新。
        /// 这个方法不会回收列表项，如果你需要大量或者频繁的创建列表项，请使用VirtualListLayoutGroup。
        /// </summary>
        /// <param name="listRoot"></param>
        /// <param name="listItemTemplate"></param>
        /// <param name="itemList"></param>
        /// <param name="count"></param>
        /// <param name="onUpdate"></param>
        /// <param name="onCreate"></param>
        /// <param name="onDestroy"></param>
        public static void updateList(RectTransform listRoot, RectTransform listItemTemplate, List<RectTransform> itemList, int count, Action<int, RectTransform> onUpdate,
            Action<RectTransform> onCreate = null,
            Action<RectTransform> onDestroy = null)
        {
            if (itemList.Count < count)
            {
                //创建列表项，使用for循环避免死循环
                int n = count - itemList.Count;
                for (int i = 0; i < n; i++)
                {
                    RectTransform item = UnityEngine.Object.Instantiate(listItemTemplate, listRoot);
                    item.gameObject.SetActive(true);
                    itemList.Add(item);
                    if (onCreate != null)
                        onCreate(item);
                }
            }
            else if (itemList.Count > count)
            {
                //销毁列表项
                int n = itemList.Count - count;
                for (int i = 0; i < n; i++)
                {
                    RectTransform item = itemList[itemList.Count - 1];
                    UnityEngine.Object.Destroy(item.gameObject);
                    itemList.RemoveAt(itemList.Count - 1);
                    if (onDestroy != null)
                        onDestroy(item);
                }
            }
            //更新列表项
            for (int i = 0; i < count; i++)
            {
                RectTransform item = itemList[i];
                if (onUpdate != null)
                    onUpdate(i, item);
            }
        }
    }
}