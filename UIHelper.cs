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
        /// <param name="rebuild">是否要立即重新构建UI列表。</param>
        public static void updateList(RectTransform listRoot, RectTransform listItemTemplate, List<RectTransform> itemList, int count, Action<int, RectTransform> onUpdate,
            Action<RectTransform> onCreateOrEnable = null, Action<RectTransform> onDestroyOrDisable = null, bool dontDestroy = false, bool rebuild = false)
        {
            if (listItemTemplate.transform.parent == listRoot)
            {
                listItemTemplate.gameObject.SetActive(false);
            }

            int maxNum = Math.Max(itemList.Count, count);

            for (int i = 0; i < maxNum; i++)
            {
                if (i < count) // 应当出现在列表中
                {
                    RectTransform item;
                    if (i >= itemList.Count) // 目前没有这个项
                    {
                        //创建列表项
                        item = Object.Instantiate(listItemTemplate, listRoot);
                        itemList.Add(item);
                        //激活
                        item.gameObject.SetActive(true);
                        onCreateOrEnable?.Invoke(item);
                    }
                    else // 目前有这个项
                    {
                        item = itemList[i];
                        if (!item.gameObject.activeSelf)
                        {
                            //激活
                            item.gameObject.SetActive(true);
                            onCreateOrEnable?.Invoke(item);
                        }
                    }
                    //更新
                    onUpdate?.Invoke(i, item);
                }
                else // 不应出现在列表中
                {
                    RectTransform item;
                    if (!dontDestroy) // 可以销毁
                    {
                        if (count < itemList.Count) // 目前有这个项
                        {
                            // 销毁列表项
                            item = itemList[count];
                            item.SetParent(null);
                            Object.Destroy(item.gameObject);
                            itemList.RemoveAt(count);
                            onDestroyOrDisable?.Invoke(item);
                        }
                    }
                    else // 不可以销毁
                    {
                        if (i < itemList.Count) // 目前有这个项
                        {
                            item = itemList[i];
                            if (item.gameObject.activeSelf)
                            {
                                // 禁用
                                item.gameObject.SetActive(false);
                                onDestroyOrDisable?.Invoke(item);
                            }
                        }
                    }
                }
            }
            if (!rebuild)
                return;
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