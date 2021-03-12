using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
namespace BJSYGameCore
{
    public static class UIHelper
    {
        /// <summary>
        /// 刷新列表
        /// </summary>
        /// <typeparam name="T">列表控件类型</typeparam>
        /// <param name="template">列表项模板，用于实例化新项</param>
        /// <param name="itemRoot">列表项所处根节点</param>
        /// <param name="itemList">控件列表</param>
        /// <param name="itemCount">要刷新的控件数量</param>
        /// <param name="onCreate">当实例化新项时调用，需要对新项进行初始化并返回控件</param>
        /// <param name="onUpdate">当刷新项目时调用，可以对项目进行刷新</param>
        public static void updateList<T>(this List<T> itemList, GameObject template, Transform itemRoot, int itemCount, Func<GameObject, T> onCreate, Action<T, int> onUpdate) where T : Component
        {
            for (int i = 0; i < itemCount; i++)
            {
                if (i >= itemList.Count)
                {
                    GameObject item = Object.Instantiate(template);
                    item.transform.SetParent(itemRoot, false);
                    itemList.Add(onCreate(item));
                }
                itemList[i].gameObject.SetActive(true);
                if (onUpdate != null)
                    onUpdate(itemList[i], i);
            }
            for (int i = itemCount; i < itemList.Count; i++)
            {
                itemList[i].gameObject.SetActive(false);
            }
        }
        /// <summary>
        /// 刷新列表
        /// </summary>
        /// <typeparam name="T">列表控件类型</typeparam>
        /// <param name="template">列表项模板，用于实例化新项</param>
        /// <param name="itemRoot">列表项所处根节点</param>
        /// <param name="itemList">控件列表</param>
        /// <param name="itemCount">要刷新的控件数量</param>
        /// <param name="onCreate">当实例化新项时调用，需要对新项进行初始化并返回控件</param>
        /// <param name="onUpdate">当刷新项目时调用，可以对项目进行刷新</param>
        public static void updateList<TCtrl, TViewData>(this List<TCtrl> itemList, GameObject template, Transform itemRoot, TViewData[] viewDatas, Func<GameObject, TCtrl> onCreate, Action<TCtrl, TViewData> onUpdate, Action<TCtrl> onRecycle) where TCtrl : Component
        {
            for (int i = 0; i < viewDatas.Length; i++)
            {
                if (i >= itemList.Count)
                {
                    GameObject item = Object.Instantiate(template);
                    item.transform.SetParent(itemRoot, false);
                    itemList.Add(onCreate(item));
                }
                itemList[i].gameObject.SetActive(true);
                if (onUpdate != null)
                    onUpdate(itemList[i], viewDatas[i]);
            }
            for (int i = viewDatas.Length; i < itemList.Count; i++)
            {
                onRecycle(itemList[i]);
                itemList[i].gameObject.SetActive(false);
            }
        }
    }
}