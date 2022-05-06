using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.UI;
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
        /// <param name="onCreateOrEnable"></param>
        /// <param name="onDestroyOrDisable"></param>
        /// <param name="dontDestroy">不摧毁列表项物体，如果该值为真，则物体将不会被摧毁而是被隐藏。</param>
        public static void updateList(RectTransform listRoot, RectTransform listItemTemplate, List<RectTransform> itemList, int count, Action<int, RectTransform> onUpdate,
            Action<RectTransform> onCreateOrEnable = null, Action<RectTransform> onDestroyOrDisable = null, bool dontDestroy = false)
        {
            if (listItemTemplate.transform.parent == listRoot)
            {
                listItemTemplate.gameObject.SetActive(false);
            }
            if (itemList.Count < count)
            {
                //创建列表项，使用for循环避免死循环
                int n = count - itemList.Count;
                for (int i = 0; i < n; i++)
                {
                    RectTransform item = UnityEngine.Object.Instantiate(listItemTemplate, listRoot);
                    item.gameObject.SetActive(true);
                    itemList.Add(item);
                    if (!dontDestroy)
                    {
                        //在摧毁物体的情况下，这个回调作为onCreate被调用。
                        onCreateOrEnable?.Invoke(item);
                    }
                }
            }
            else if (itemList.Count > count && !dontDestroy)
            {
                //销毁列表项
                int n = itemList.Count - count;
                for (int i = 0; i < n; i++)
                {
                    RectTransform item = itemList[itemList.Count - 1];
                    item.SetParent(null);
                    UnityEngine.Object.Destroy(item.gameObject);
                    itemList.RemoveAt(itemList.Count - 1);
                    onDestroyOrDisable?.Invoke(item);
                }
            }
            //更新列表项
            for (int i = 0; i < itemList.Count; i++)
            {
                RectTransform item = itemList[i];
                if (dontDestroy)
                {
                    //在不摧毁物体的情况下，要更新物体的激活状态
                    if (i < count)
                    {
                        //激活
                        item.gameObject.SetActive(true);
                        onCreateOrEnable?.Invoke(item);
                        //更新
                        onUpdate?.Invoke(i, item);
                    }
                    else
                    {
                        //禁用
                        item.gameObject.SetActive(false);
                        onDestroyOrDisable?.Invoke(item);
                    }
                }
                else
                {
                    onUpdate?.Invoke(i, item);
                }
            }
            foreach (var layoutGroup in listRoot.GetComponentsInChildren<LayoutGroup>())
            {
                if (layoutGroup.transform == listRoot)
                    continue;
                LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.transform as RectTransform);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(listRoot);
            foreach (var layoutGroup in listRoot.GetComponentsInParent<LayoutGroup>())
            {
                if (layoutGroup.transform == listRoot)
                    continue;
                LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.transform as RectTransform);
            }
        }
        /// <summary>
        /// 刷新列表，在根节点下创建或销毁列表项直到指定数量，然后调用回调来进行更新。
        /// 这个方法不会回收列表项，如果你需要大量或者频繁的创建列表项，请使用VirtualListLayoutGroup。
        /// </summary>
        /// <param name="listRoot"></param>
        /// <param name="listItemTemplate"></param>
        /// <param name="itemList"></param>
        /// <param name="count"></param>
        /// <param name="onUpdate"></param>
        /// <param name="onCreateOrEnable"></param>
        /// <param name="onDestroyOrDisable"></param>
        /// <param name="dontDestroy">不摧毁列表项物体，如果该值为真，则物体将不会被摧毁而是被隐藏。</param>
        public static void updateList<T>(RectTransform listRoot, T listItemTemplate, List<T> itemList, int count, Action<int, T> onUpdate,
            Action<T> onCreateOrEnable = null, Action<T> onDestroyOrDisable = null, bool dontDestroy = false) where T : MonoBehaviour
        {
            if (listItemTemplate.transform.parent == listRoot)
            {
                listItemTemplate.gameObject.SetActive(false);
            }
            if (itemList.Count < count)
            {
                //创建列表项，使用for循环避免死循环
                int n = count - itemList.Count;
                for (int i = 0; i < n; i++)
                {
                    T item = Object.Instantiate(listItemTemplate, listRoot);
                    item.gameObject.SetActive(true);
                    itemList.Add(item);
                    if (!dontDestroy)
                    {
                        //在摧毁物体的情况下，这个回调作为onCreate被调用。
                        onCreateOrEnable?.Invoke(item);
                    }
                }
            }
            else if (itemList.Count > count && !dontDestroy)
            {
                //销毁列表项
                int n = itemList.Count - count;
                for (int i = 0; i < n; i++)
                {
                    T item = itemList[itemList.Count - 1];
                    item.transform.SetParent(null);
                    Object.Destroy(item.gameObject);
                    itemList.RemoveAt(itemList.Count - 1);
                    onDestroyOrDisable?.Invoke(item);
                }
            }
            //更新列表项
            for (int i = 0; i < itemList.Count; i++)
            {
                T item = itemList[i];
                if (dontDestroy)
                {
                    //在不摧毁物体的情况下，要更新物体的激活状态
                    if (i < count)
                    {
                        //激活
                        item.gameObject.SetActive(true);
                        onCreateOrEnable?.Invoke(item);
                        //更新
                        onUpdate?.Invoke(i, item);
                    }
                    else
                    {
                        //禁用
                        item.gameObject.SetActive(false);
                        onDestroyOrDisable?.Invoke(item);
                    }
                }
                else
                {
                    onUpdate?.Invoke(i, item);
                }
            }
            foreach (var layoutGroup in listRoot.GetComponentsInChildren<LayoutGroup>())
            {
                if (layoutGroup.transform == listRoot)
                    continue;
                LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.transform as RectTransform);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(listRoot);
            foreach (var layoutGroup in listRoot.GetComponentsInParent<LayoutGroup>())
            {
                if (layoutGroup.transform == listRoot)
                    continue;
                LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.transform as RectTransform);
            }
        }
    }
}