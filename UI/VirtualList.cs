// Author: 闲玩鸭
// Contact: 2041744819@qq.com
// Date: 2021/4/2

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BJSYGameCore.UI
{
    /// <summary>
    /// 虚拟列表
    /// </summary>
    /// <typeparam name="T"> UI物体的类型 </typeparam>
    public class VirtualList<T> where T : MonoBehaviour
    {
        private struct UIElement
        {
            public T uiObj;
            public RectTransform rectTransform;
        }

        private enum LayoutGroupType
        {
            Gird,
            Horizontal,
            Vertical,
        }

        //由于Vertical和Horizontal类型的LayoutGroup
        //没有提供UI物体的尺寸信息
        //这里需要从外部获取一下
        public RectTransform listUIObjRectTrans;

        private LayoutGroupType layoutGroupType;
        private RectTransform layoutGroupRectTrans;
        private ScrollRect scrollRect;

        /// <summary>
        /// 刷新列表中UI物体时，需要触发此事件
        /// </summary>
        public event Action<int, T> onDisplayUIObj;

        /// <summary>
        /// 实际显示的UI物体和它的RectTransform
        /// </summary>
        private LinkedList<UIElement> uiElements = new LinkedList<UIElement>();

        /// <summary>
        /// UI物体生成器，UI物体生成方法需要由外面指定
        /// </summary>
        private Func<T> uiObjGernerator;
        /// <summary>
        /// 最后一个UI物体的索引
        /// </summary>
        private int rearUIObjIndex = -1;
        /// <summary>
        /// 数据的总量
        /// </summary>
        private int totalDataCount = 0;
        /// <summary>
        /// 考虑了spacing的列表中UI物体尺寸
        /// </summary>
        private Vector2 realCellSize = Vector2.zero;
        /// <summary>
        /// 记录上一次滚动时，列表横行或纵行的行数
        /// </summary>
        private int lastLineCount = 0;

        public RectTransform ListUIObjRectTrans
        {
            get
            {
                if (listUIObjRectTrans == null)
                {
                    listUIObjRectTrans = layoutGroupRectTrans.GetChild(0).GetComponent<RectTransform>();
                }
                return listUIObjRectTrans;
            }
        }
        /// <summary>
        /// 横向UI物体个数
        /// </summary>
        public int HorizontalCount { get; private set; }
        /// <summary>
        /// 纵向UI物体个数
        /// </summary>
        public int VerticalCount { get; private set; }
        /// <summary>
        /// 总共创建的UI物体的个数
        /// </summary>
        public int TotalElementCount { get { return HorizontalCount * VerticalCount; } }
        public int TotalDataCount
        {
            get { return totalDataCount; }
            set
            {
                totalDataCount = value;

                // 对ContentSizeFitter做一下容错
                //强行将ContentSizeFitter的所有选项设为Unconstrained模式，以便能调整LayoutGroup的大小
                ContentSizeFitter csf = layoutGroupRectTrans.GetComponent<ContentSizeFitter>();
                if (csf)
                {
                    csf.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
                    csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                }


                // 判断LayoutGroup的类型，然后计算LayoutGroup的尺寸
                switch (layoutGroupType)
                {
                    case LayoutGroupType.Horizontal:
                        //锚点设在左侧
                        layoutGroupRectTrans.anchorMin = Vector2.zero;
                        layoutGroupRectTrans.anchorMax = Vector2.up;

                        layoutGroupRectTrans.setWidth(value * realCellSize.x);
                        break;
                    case LayoutGroupType.Vertical:
                        //锚点设在顶端
                        layoutGroupRectTrans.anchorMin = Vector2.up;
                        layoutGroupRectTrans.anchorMax = Vector2.one;

                        layoutGroupRectTrans.setHeight(value * realCellSize.y);
                        break;
                    case LayoutGroupType.Gird:
                        //锚点设在左上角
                        layoutGroupRectTrans.anchorMin = layoutGroupRectTrans.anchorMax = Vector2.up;

                        if (scrollRect.horizontal && !scrollRect.vertical)
                        {
                            int colCount = Mathf.CeilToInt((float)value / VerticalCount);
                            layoutGroupRectTrans.setWidth(colCount * realCellSize.x);
                            layoutGroupRectTrans.setHeight(scrollRect.GetComponent<RectTransform>().rect.height);
                        }
                        else if (scrollRect.vertical && !scrollRect.horizontal)
                        {
                            int rowCount = Mathf.CeilToInt((float)value / HorizontalCount);
                            layoutGroupRectTrans.setWidth(scrollRect.GetComponent<RectTransform>().rect.width);
                            layoutGroupRectTrans.setHeight(rowCount * realCellSize.y);
                        }
                        break;
                }
                layoutGroupRectTrans.anchoredPosition = Vector2.zero;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="itemGernerator">ui物体生成器</param>
        /// <param name="layoutGroup">layoutGroup</param>
        public VirtualList(Func<T> itemGernerator, LayoutGroup layoutGroup)
        {
            layoutGroupRectTrans = layoutGroup.GetComponent<RectTransform>();
            scrollRect = layoutGroupRectTrans.GetComponentInParent<ScrollRect>();
            if (scrollRect == null)
            {
                Debug.LogError("ScrollRect should not be null in parent obj!!! \n (父物体里ScrollRect不能为空)");
                return;
            }
            Rect viewPortRect = scrollRect.GetComponent<RectTransform>().rect;
            //考虑了padding在内计算出的实际视口的宽和高
            float realHeight = viewPortRect.height - layoutGroup.padding.top - layoutGroup.padding.bottom;
            float realWidth = viewPortRect.width - layoutGroup.padding.left - layoutGroup.padding.right;

            if (layoutGroup is GridLayoutGroup)
            {
                layoutGroupType = LayoutGroupType.Gird;
                GridLayoutGroup gridLayoutGroup = layoutGroup as GridLayoutGroup;

                // 考虑spacing计算列表中UI物体尺寸
                float cellWidth = gridLayoutGroup.cellSize.x + gridLayoutGroup.spacing.x;
                float cellHeight = gridLayoutGroup.cellSize.y + gridLayoutGroup.spacing.y;
                realCellSize = new Vector2(cellWidth, cellHeight);
                //计算横行和纵行的个数
                HorizontalCount = (int)((realWidth + gridLayoutGroup.spacing.x) / realCellSize.x);
                VerticalCount = (int)((realHeight + gridLayoutGroup.spacing.y) / realCellSize.y);
                HorizontalCount = HorizontalCount == 0 ? 1 : HorizontalCount;
                VerticalCount = VerticalCount == 0 ? 1 : VerticalCount;

                //对gridLayoutGroup做一下容错
                if (scrollRect.horizontal && !scrollRect.vertical)
                {
                    gridLayoutGroup.startAxis = GridLayoutGroup.Axis.Vertical;
                    HorizontalCount += 2;
                }
                else if (scrollRect.vertical && !scrollRect.horizontal)
                {
                    gridLayoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
                    VerticalCount += 2;
                }
                else
                {
                    Debug.LogError("VirtualList don't work when both horizontal and vertical Mode of ScrollRect activated \n" +
                      "（这个虚拟列表不支持ScrollRect的horizontal和vertical同时勾选的情况）");
                    return;
                }
                //gridLayoutGroup只能从左上角开始
                gridLayoutGroup.startCorner = GridLayoutGroup.Corner.UpperLeft;
            }
            else if (layoutGroup is VerticalLayoutGroup)
            {
                layoutGroupType = LayoutGroupType.Vertical;
                VerticalLayoutGroup verticalLayoutGroup = layoutGroup as VerticalLayoutGroup;

                // 考虑spacing计算列表中UI物体尺寸
                float cellWidth = ListUIObjRectTrans.rect.width * (verticalLayoutGroup.childScaleWidth ? ListUIObjRectTrans.localScale.x : 1);
                float cellHeight = ListUIObjRectTrans.rect.height * (verticalLayoutGroup.childScaleHeight ? listUIObjRectTrans.localScale.y : 1) + verticalLayoutGroup.spacing;
                realCellSize = new Vector2(cellWidth, cellHeight);
                //计算横行和纵行的个数
                HorizontalCount = 1;
                VerticalCount = 2 + (int)((realHeight + verticalLayoutGroup.spacing) / realCellSize.y);

                //对verticalLayoutGroup做一下容错设置
                verticalLayoutGroup.childForceExpandHeight = false;
                verticalLayoutGroup.childControlHeight = false;
                //verticalLayoutGroup只能是从上到下
                switch (verticalLayoutGroup.childAlignment)
                {
                    case TextAnchor.MiddleCenter:
                    case TextAnchor.LowerCenter:
                        verticalLayoutGroup.childAlignment = TextAnchor.UpperCenter;
                        break;
                    case TextAnchor.MiddleLeft:
                    case TextAnchor.LowerLeft:
                        verticalLayoutGroup.childAlignment = TextAnchor.UpperLeft;
                        break;
                    case TextAnchor.MiddleRight:
                    case TextAnchor.LowerRight:
                        verticalLayoutGroup.childAlignment = TextAnchor.UpperRight;
                        break;
                }
            }
            else if (layoutGroup is HorizontalLayoutGroup)
            {
                layoutGroupType = LayoutGroupType.Horizontal;
                HorizontalLayoutGroup horizontalLayoutGroup = layoutGroup as HorizontalLayoutGroup;

                // 考虑spacing计算列表中UI物体尺寸
                float cellWidth = ListUIObjRectTrans.rect.width * (horizontalLayoutGroup.childScaleWidth ? ListUIObjRectTrans.localScale.x : 1) + horizontalLayoutGroup.spacing;
                float cellHeight = ListUIObjRectTrans.rect.height * (horizontalLayoutGroup.childScaleHeight ? listUIObjRectTrans.localScale.y : 1);
                realCellSize = new Vector2(cellWidth, cellHeight);
                //计算横行和纵行的个数
                VerticalCount = 1;
                HorizontalCount = 2 + (int)((realWidth + horizontalLayoutGroup.spacing) / realCellSize.x);

                //对horizontalLayoutGroup做一下容错设置
                horizontalLayoutGroup.childForceExpandWidth = false;
                horizontalLayoutGroup.childControlWidth = false;
                horizontalLayoutGroup.childScaleWidth = true;
                //horizontalLayoutGroup只能是从左到右
                switch (horizontalLayoutGroup.childAlignment)
                {
                    case TextAnchor.UpperLeft:
                    case TextAnchor.UpperCenter:
                        horizontalLayoutGroup.childAlignment = TextAnchor.UpperRight;
                        break;
                    case TextAnchor.MiddleLeft:
                    case TextAnchor.MiddleCenter:
                        horizontalLayoutGroup.childAlignment = TextAnchor.MiddleRight;
                        break;
                    case TextAnchor.LowerLeft:
                    case TextAnchor.LowerCenter:
                        horizontalLayoutGroup.childAlignment = TextAnchor.LowerRight;
                        break;
                }
            }
            else
            {
                Debug.LogError("The param of layoutGroup must be a subclass object of LayoutGroup \n" +
                    "参数layoutGroup必须是LayoutGroup的子类对象");
                return;
            }

            scrollRect.onValueChanged.AddListener(onScrollDrag);
            this.uiObjGernerator = itemGernerator;
        }

        /// <summary>
        /// 添加UI物体
        /// 这里叫addItem是为了和外面的接口名字一样.....
        /// </summary>
        /// <returns>UI物体实例</returns>
        public T addItem()
        {
            if (uiElements.Count < TotalElementCount)
            {
                T uiObj = uiObjGernerator?.Invoke();
                if (uiObjGernerator == null)
                {
                    Debug.LogError("UIObjGernerator should not be null!!! \n" +
                        "UIObjGernerator 不应该为空");
                }

                RectTransform objRectTransform = uiObj.GetComponent<RectTransform>();
                if (objRectTransform == null)
                {
                    Debug.LogError("UI obj must have RectTransfrom, but it didn't!!! \n" +
                        "UI物体一定有RectTransform，但是它没有");
                    return null;
                }

                uiElements.AddLast(new UIElement { uiObj = uiObj, rectTransform = objRectTransform });
                onDisplayUIObj?.Invoke(++rearUIObjIndex, uiObj);
                return uiObj;
            }
            return null;
        }

        /// <summary>
        /// 列表滚动回调
        /// </summary>
        /// <param name="_"></param>
        void onScrollDrag(Vector2 _)
        {
            switch (layoutGroupType)
            {
                case LayoutGroupType.Vertical:
                    updateVertical();
                    break;
                case LayoutGroupType.Horizontal:
                    updateHorizontal();
                    break;
                case LayoutGroupType.Gird:
                    if (scrollRect.horizontal && !scrollRect.vertical) { updateHorizontal(); }
                    else if (scrollRect.vertical && !scrollRect.horizontal) { updateVertical(); }
                    break;
            }
        }

        /// <summary>
        /// 纵向滚动刷新
        /// </summary>
        void updateVertical()
        {
            int lastRearUIObjIndex = rearUIObjIndex;
            int lineCount = Mathf.FloorToInt(Mathf.Abs(layoutGroupRectTrans.anchoredPosition.y) / realCellSize.y);
            int deltaLineCount = lineCount - lastLineCount;
            rearUIObjIndex += deltaLineCount * HorizontalCount;
            Vector2 deltaPos = new Vector2(0, realCellSize.y * VerticalCount);

            // 列表向下翻
            if (deltaLineCount > 0)
            {
                for (int uiObjIndex = lastRearUIObjIndex + 1; uiObjIndex <= rearUIObjIndex; uiObjIndex++)
                {
                    //防止数组越界
                    if (uiObjIndex >= TotalDataCount || uiObjIndex < TotalElementCount) { continue; }

                    //刷新位置和数据
                    UIElement element = uiElements.First.Value;
                    uiElements.RemoveFirst();
                    element.rectTransform.anchoredPosition -= deltaPos;
                    uiElements.AddLast(element);
                    onDisplayUIObj?.Invoke(uiObjIndex, element.uiObj);

                }
            }
            // 列表向上翻
            else if (deltaLineCount < 0)
            {
                for (int uiObjIndex = lastRearUIObjIndex; uiObjIndex > rearUIObjIndex; uiObjIndex--)
                {
                    //防止数组越界
                    if (uiObjIndex >= TotalDataCount || uiObjIndex < TotalElementCount) { continue; }

                    //刷新位置和数据
                    UIElement element = uiElements.Last.Value;
                    uiElements.RemoveLast();
                    element.rectTransform.anchoredPosition += deltaPos;
                    uiElements.AddFirst(element);
                    onDisplayUIObj?.Invoke(uiObjIndex - TotalElementCount, element.uiObj);
                }
            }
            lastLineCount = lineCount;
        }

        /// <summary>
        /// 横向滚动刷新
        /// </summary>
        void updateHorizontal()
        {
            int lastRearUIObjIndex = rearUIObjIndex;
            int lineCount = Mathf.FloorToInt(Mathf.Abs(layoutGroupRectTrans.anchoredPosition.x) / realCellSize.x);
            int deltaLineCount = lineCount - lastLineCount;
            rearUIObjIndex += deltaLineCount * VerticalCount;
            Vector2 deltaPos = new Vector2(realCellSize.x * HorizontalCount, 0);

            //列表向右翻
            if (deltaLineCount > 0)
            {
                for (int uiObjIndex = lastRearUIObjIndex + 1; uiObjIndex <= rearUIObjIndex; uiObjIndex++)
                {
                    //防止数组越界
                    if (uiObjIndex >= TotalDataCount || uiObjIndex < TotalElementCount) { continue; }

                    //刷新位置和数据
                    UIElement element = uiElements.First.Value;
                    uiElements.RemoveFirst();
                    element.rectTransform.anchoredPosition += deltaPos;
                    uiElements.AddLast(element);
                    onDisplayUIObj?.Invoke(uiObjIndex, element.uiObj);
                }
            }
            //列表向左翻
            else if (deltaLineCount < 0)
            {
                for (int uiObjIndex = lastRearUIObjIndex; uiObjIndex > rearUIObjIndex; uiObjIndex--)
                {
                    //防止数组越界
                    if (uiObjIndex >= TotalDataCount || uiObjIndex < TotalElementCount) { continue; }

                    //刷新位置和数据
                    UIElement element = uiElements.Last.Value;
                    uiElements.RemoveLast();
                    element.rectTransform.anchoredPosition -= deltaPos;
                    uiElements.AddFirst(element);
                    onDisplayUIObj?.Invoke(uiObjIndex - TotalElementCount, element.uiObj);
                }
            }
            lastLineCount = lineCount;
        }

        /// <summary>
        /// 重载整个视口里面的元素
        /// </summary>
        public void ReloadViewportElements()
        {
            var ele = uiElements.Last;
            for (int i = 0; ele != null; i++, ele = ele.Previous)
            {
                var index = rearUIObjIndex - i;
                if (index < 0 || index >= TotalDataCount) continue;
                onDisplayUIObj?.Invoke(index, ele.Value.uiObj);
            }
        }

        /// <summary>
        /// 重载在指定位置的元素
        /// </summary>
        /// <param name="index"></param>
        public void ReloadElementAt(int index)
        {
            var delta = rearUIObjIndex - index;
            if (delta < 0 || delta >= uiElements.Count) return;

            var ele = uiElements.Last;
            for (int i = 0; i < delta; i++, ele = ele.Previous) ;

            onDisplayUIObj?.Invoke(index, ele.Value.uiObj);
        }
    }
}
