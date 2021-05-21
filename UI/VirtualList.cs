// Author: 闲玩鸭
// Contact: 2041744819@qq.com
// Date: 2021/4/2

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace BJSYGameCore.UI {
    /// <summary>
    /// 虚拟列表（暂时只支持从上到下和从左到右的排序）
    /// 相对比较静态，如果要增删元素，得reset重建一下列表....
    /// </summary>
    /// <typeparam name="T"> UI物体的类型 </typeparam>
    public class VirtualList<T> where T : MonoBehaviour {
        private class UIElement {
            public T uiObj;
            public RectTransform rectTransform;
        }

        private enum LayoutGroupType {
            Gird,
            Horizontal,
            Vertical,
            None,
        }

        private class UIElementCache {
            /// <summary>
            /// ui物体缓存
            /// </summary>
            private List<UIElement> cache = new List<UIElement>();
            /// <summary>
            /// 可用的索引
            /// </summary>
            int availiableIndex = 0;
            /// <summary>
            /// 缓存新产生的UI物体
            /// </summary>
            /// <param name="element">new</param>
            public void cacheUIElement(UIElement element) {
                cache.Add(element);
                availiableIndex = cache.Count;
            }
            /// <summary>
            /// 让缓存生效
            /// </summary>
            public void makeCacheAvailiable() {
                for (int elementIndex = 0; elementIndex < cache.Count; elementIndex++) {
                    UIElement element = cache[elementIndex];
                    element.uiObj.gameObject.SetActive(false);
                }
                availiableIndex = 0;
            }
            /// <summary>
            /// 从缓存中获取可用的UI物体
            /// </summary>
            /// <returns></returns>
            public UIElement getAvailiableUIElement() {
                if (availiableIndex < cache.Count) {
                    var element = cache[availiableIndex];
                    element.uiObj.gameObject.SetActive(true);
                    availiableIndex++;
                    return element;
                }
                return null;
            }
            /// <summary>
            /// 象征性的clear
            /// </summary>
            public void clear() {
                cache.Clear();
                availiableIndex = 0;
            }
        }

        private RectTransform listUIObjRectTrans;
        private LayoutGroupType layoutGroupType;
        private RectTransform layoutGroupRectTrans;
        private ScrollRect scrollRect;
        private RectTransform scrollRectTrans;
        private LayoutGroup layoutGroup;
        private Rect currViewPortRect = Rect.zero;

        /// <summary>
        /// 刷新列表中UI物体时，需要触发此事件
        /// </summary>
        public event Action<int, T> onDisplayUIObj;

        /// <summary>
        /// 实际显示的UI物体
        /// </summary>
        private LinkedList<UIElement> uiElements = new LinkedList<UIElement>();
        /// <summary>
        /// ui物体缓存池
        /// </summary>
        private UIElementCache uiElementCache = new UIElementCache();

        /// <summary>
        /// 使用策略模式简化init代码
        /// </summary>
        private Dictionary<LayoutGroupType, UnityAction<float,float>> initStrategyDict = new Dictionary<LayoutGroupType, UnityAction<float, float>>();

        /// <summary>
        /// UI物体生成器，UI物体生成方法需要由外面指定
        /// </summary>
        private Func<T> uiObjGernerator;
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
        /// <summary>
        /// 记录了最后一行或最后一列的最后一个ui物体的索引，可能会越界
        /// </summary>
        private int rearUIObjIndex = -1;

        //由于Vertical和Horizontal类型的LayoutGroup
        //没有提供UI物体的尺寸信息
        //这里需要从外部获取一下
        public RectTransform ListUIObjRectTrans {
            get {
                if (listUIObjRectTrans == null) {
                    if(layoutGroupRectTrans.childCount > 0) {
                        Transform listUIObjTrans = layoutGroupRectTrans.GetChild(0);
                        listUIObjTrans.gameObject.SetActive(false);
                        listUIObjRectTrans = listUIObjTrans.GetComponent<RectTransform>();
                    }
                    else {
                        Debug.LogError("There is not templete uiObject exist in layoutGroup!!\n" +
                            "（layoutGroup里面没有模板UI物体!!）");
                        return null;
                    }
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
        /// 创建的UI物体的最大个数
        /// </summary>
        public int MaxElementCount { get { return HorizontalCount * VerticalCount; } }
        public int TotalDataCount {
            get { return totalDataCount; }
            set {
                totalDataCount = value;
                resetLayoutGroupRect();
            }
        }

        public int RearUIObjIndex { get => rearUIObjIndex; }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="itemGernerator">ui物体生成器</param>
        /// <param name="layoutGroup">layoutGroup</param>
        public VirtualList(Func<T> uiObjGernerator, LayoutGroup layoutGroup) {
            this.uiObjGernerator = uiObjGernerator;
            this.layoutGroup = layoutGroup;
            layoutGroupRectTrans = layoutGroup.GetComponent<RectTransform>();
            scrollRect = layoutGroupRectTrans.GetComponentInParent<ScrollRect>();
            if (scrollRect == null) {
                Debug.LogError("ScrollRect should not be null in parent obj of layoutGroup!!! \n (layoutGroup父物体里ScrollRect不能为空)");
                return;
            }
            scrollRectTrans = scrollRect.GetComponent<RectTransform>();
            uiElementCache = new UIElementCache();
            uiElements = new LinkedList<UIElement>();
            initStrategyDict = new Dictionary<LayoutGroupType, UnityAction<float, float>>();
            initStrategyDict.Add(LayoutGroupType.Gird, initForGridLayoutGroup);
            initStrategyDict.Add(LayoutGroupType.Vertical, initForVerticalLayoutGroup);
            initStrategyDict.Add(LayoutGroupType.Horizontal, initForHorizontalLayoutGroup);
            init();
        }
        
        private void initForGridLayoutGroup(float realWidth, float realHeight) {
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
            if (scrollRect.horizontal && !scrollRect.vertical) {
                gridLayoutGroup.startAxis = GridLayoutGroup.Axis.Vertical;
                HorizontalCount += 2;
            }
            else if (scrollRect.vertical && !scrollRect.horizontal) {
                gridLayoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
                VerticalCount += 2;
            }
            else {
                Debug.LogError("VirtualList don't work when both horizontal and vertical Mode of ScrollRect activated or unactivated\n" +
                  "（这个虚拟列表不支持ScrollRect的horizontal和vertical同时勾选或不勾选的情况）");
                return;
            }
            //gridLayoutGroup只能从左上角开始
            gridLayoutGroup.startCorner = GridLayoutGroup.Corner.UpperLeft;
        }

        private void initForVerticalLayoutGroup(float realWidth, float realHeight) {
            VerticalLayoutGroup verticalLayoutGroup = layoutGroup as VerticalLayoutGroup;
            if (ListUIObjRectTrans == null) { return; }
            // 考虑spacing计算列表中UI物体尺寸
            float cellWidth = ListUIObjRectTrans.rect.width * (verticalLayoutGroup.childScaleWidth ? ListUIObjRectTrans.localScale.x : 1);
            float cellHeight = ListUIObjRectTrans.rect.height * (verticalLayoutGroup.childScaleHeight ? listUIObjRectTrans.localScale.y : 1);
            cellHeight += verticalLayoutGroup.spacing;
            realCellSize = new Vector2(cellWidth, cellHeight);
            //计算横行和纵行的个数
            HorizontalCount = 1;
            VerticalCount = 2 + (int)((realHeight + verticalLayoutGroup.spacing) / realCellSize.y);

            //对verticalLayoutGroup做一下容错设置
            verticalLayoutGroup.childForceExpandHeight = false;
            verticalLayoutGroup.childControlHeight = false;
            //verticalLayoutGroup只能是从上到下
            switch (verticalLayoutGroup.childAlignment) {
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

        private void  initForHorizontalLayoutGroup(float realWidth, float realHeight) {
            HorizontalLayoutGroup horizontalLayoutGroup = layoutGroup as HorizontalLayoutGroup;
            if (ListUIObjRectTrans == null) { return; }
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
            switch (horizontalLayoutGroup.childAlignment) {
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

        /// <summary>
        /// 初始化虚拟列表
        /// </summary>
        private void init() {
            currViewPortRect = scrollRectTrans.rect;
            //考虑了padding在内计算出的实际视口的宽和高
            float realHeight = currViewPortRect.height - layoutGroup.padding.top - layoutGroup.padding.bottom;
            float realWidth = currViewPortRect.width - layoutGroup.padding.left - layoutGroup.padding.right;

            // 对ContentSizeFitter做一下容错
            //强行将ContentSizeFitter的所有选项设为Unconstrained模式，以便能调整LayoutGroup的大小
            ContentSizeFitter csf = layoutGroupRectTrans.GetComponent<ContentSizeFitter>();
            if (csf) {
                csf.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
                csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            }

            layoutGroupType = LayoutGroupType.None;
            if (layoutGroup is GridLayoutGroup) {layoutGroupType = LayoutGroupType.Gird; }
            else if (layoutGroup is VerticalLayoutGroup) {layoutGroupType = LayoutGroupType.Vertical; }
            else if (layoutGroup is HorizontalLayoutGroup) {  layoutGroupType = LayoutGroupType.Horizontal;}

            if (initStrategyDict.TryGetValue(layoutGroupType, out var initFunc)) { 
                initFunc?.Invoke(realWidth, realHeight); 
            }
            else {
                Debug.LogError("layoutGroup must be a subclass object that inherits from the LayoutGroup(class) \n" +
                    "layoutGroup必须是继承自LayoutGroup(类)的子类对象");
                return;
            }
            scrollRect.onValueChanged.AddListener(updateVirtualList); 
        }

        /// <summary>
        /// 重新计算layoutGroup的尺寸
        /// </summary>
        private void resetLayoutGroupRect() {
            // 判断LayoutGroup的类型，然后计算LayoutGroup的尺寸
            switch (layoutGroupType) {
                case LayoutGroupType.Horizontal:
                    //锚点设在左侧
                    layoutGroupRectTrans.anchorMin = Vector2.zero;
                    layoutGroupRectTrans.anchorMax = Vector2.up;

                    layoutGroupRectTrans.setWidth(totalDataCount * realCellSize.x);
                    break;
                case LayoutGroupType.Vertical:
                    //锚点设在顶端
                    layoutGroupRectTrans.anchorMin = Vector2.up;
                    layoutGroupRectTrans.anchorMax = Vector2.one;

                    layoutGroupRectTrans.setHeight(totalDataCount * realCellSize.y);
                    break;
                case LayoutGroupType.Gird:
                    if (scrollRect.horizontal && !scrollRect.vertical) {
                        //锚点设在左侧
                        layoutGroupRectTrans.anchorMin = Vector2.zero;
                        layoutGroupRectTrans.anchorMax = Vector2.up;
                        int colCount = Mathf.CeilToInt((float)totalDataCount / VerticalCount);
                        layoutGroupRectTrans.setWidth(colCount * realCellSize.x);
                    }
                    else if (scrollRect.vertical && !scrollRect.horizontal) {
                        //锚点设在顶端
                        layoutGroupRectTrans.anchorMin = Vector2.up;
                        layoutGroupRectTrans.anchorMax = Vector2.one;
                        int rowCount = Mathf.CeilToInt((float)totalDataCount / HorizontalCount);
                        layoutGroupRectTrans.setHeight(rowCount * realCellSize.y);
                    }
                    break;
            }
            layoutGroupRectTrans.anchoredPosition = Vector2.zero;
        }

        /// <summary>
        /// 当ScrollRect尺寸发生变化，重新初始化虚拟列表
        /// </summary>
        public void checkViewPortChange() {
            if(currViewPortRect != scrollRectTrans.rect) {
                init();
                reset();
                resetLayoutGroupRect();
                for (int i = 0; i < totalDataCount; i++) { addItem();}
                currViewPortRect = scrollRectTrans.rect;
            }
        }

        /// <summary>
        /// 添加UI物体
        /// 这里叫addItem是为了和外面的接口名字一样.....
        /// </summary>
        /// <returns>UI物体实例</returns>
        public T addItem() {
            if (uiElements.Count < MaxElementCount) {
                UIElement element = uiElementCache.getAvailiableUIElement();
                if (element == null) {
                    T uiObj = uiObjGernerator?.Invoke();
                    if (uiObjGernerator == null) {
                        Debug.LogError("UIObjGernerator should not be null!!! \n UIObjGernerator 不应该为空");
                        return null;
                    }
                    RectTransform objRectTransform = uiObj.GetComponent<RectTransform>();
                    if (objRectTransform == null) {
                        Debug.LogError("UI obj must have RectTransfrom, but it didn't!!! \n UI物体一定有RectTransform，但是它没有");
                        return null;
                    }
                    element = new UIElement { uiObj = uiObj, rectTransform = objRectTransform };
                    uiElementCache.cacheUIElement(element);
                }
                onDisplayUIObj?.Invoke(++rearUIObjIndex, element.uiObj);
                uiElements.AddLast(element);
                return element.uiObj;         
            }
            return null;
        }

        /// <summary>
        /// 需要往虚拟列表添加元素之前记得reset一次
        /// </summary>
        public void reset() {
            //重置部分成员
            layoutGroupRectTrans.anchoredPosition = Vector2.zero;
            rearUIObjIndex = -1;
            lastLineCount = 0;
            uiElements.Clear();
            uiElementCache.makeCacheAvailiable();
        }

        /// <summary>
        /// 列表滚动回调
        /// </summary>
        /// <param name="_"></param>
        void updateVirtualList(Vector2 _) {
            int lastRearUIObjIndex = rearUIObjIndex;//记录上一次
            int lineCount = 0;                      //当前滚动过的横行或纵行的行数
            int deltaLineCount = 0;                 //行号差
            Vector2 deltaPos = Vector2.zero;        //位置差

            if (scrollRect.vertical) {
                if (layoutGroupRectTrans.anchoredPosition.y < 0) { return; } //防止越界更新
                lineCount = Mathf.FloorToInt(Mathf.Abs(layoutGroupRectTrans.anchoredPosition.y) / realCellSize.y);
                deltaLineCount = lineCount - lastLineCount;
                rearUIObjIndex += deltaLineCount * HorizontalCount;
                deltaPos = new Vector2(0, realCellSize.y * VerticalCount);
            }
            if (scrollRect.horizontal) {
                if (layoutGroupRectTrans.anchoredPosition.x > 0) { return; } //防止越界更新
                lineCount = Mathf.FloorToInt(Mathf.Abs(layoutGroupRectTrans.anchoredPosition.x) / realCellSize.x);
                deltaLineCount = lineCount - lastLineCount;
                rearUIObjIndex += deltaLineCount * VerticalCount;
                deltaPos = -new Vector2(realCellSize.x * HorizontalCount, 0);
            }

            //正向滚动(向下or向右)
            if (deltaLineCount > 0) {
                for (int uiObjIndex = lastRearUIObjIndex + 1; uiObjIndex <= rearUIObjIndex; uiObjIndex++) {
                    //防止数组越界
                    if (uiObjIndex >= TotalDataCount || uiObjIndex < MaxElementCount) { continue; }

                    //刷新位置和数据
                    var elementNode = uiElements.First;
                    UIElement element = elementNode.Value;
                    element.rectTransform.anchoredPosition -= deltaPos;
                    uiElements.RemoveFirst();
                    uiElements.AddLast(elementNode);
                    onDisplayUIObj?.Invoke(uiObjIndex, element.uiObj);

                }
            }
            //反向滚动（向上or向左)
            else if (deltaLineCount < 0) {
                for (int uiObjIndex = lastRearUIObjIndex; uiObjIndex > rearUIObjIndex; uiObjIndex--) {
                    //防止数组越界
                    if (uiObjIndex >= TotalDataCount || uiObjIndex < MaxElementCount) { continue; }

                    //刷新位置和数据
                    var elementNode = uiElements.Last;
                    UIElement element = elementNode.Value;
                    element.rectTransform.anchoredPosition += deltaPos;
                    uiElements.RemoveLast();
                    uiElements.AddFirst(elementNode);
                    onDisplayUIObj?.Invoke(uiObjIndex - MaxElementCount, element.uiObj);
                }
            }

            lastLineCount = lineCount;
        }

        /// <summary>
        /// 重载在指定位置的元素
        /// </summary>
        /// <param name="index">数据索引</param>
        public void ReloadElementAt(int index) {
            int delta;
            if (rearUIObjIndex < TotalDataCount) { delta = rearUIObjIndex - index; }
            else { delta = TotalDataCount-1 - index; }
            if (delta < 0 || delta >= MaxElementCount) { return; }
            LinkedListNode<UIElement> ele;
            if(uiElements.Count<MaxElementCount) {
                ele = uiElements.First;
                for (int i = 1; i <= index; i++, ele = ele.Next);
            }
            else {
                ele = uiElements.Last;
                for (int i = 0; i < delta; i++, ele = ele.Previous);
            }
            onDisplayUIObj?.Invoke(index, ele.Value.uiObj);
        }

        /// <summary>
        /// 象征性的clear
        /// </summary>
        public void clear() {
            reset();
            uiElements.Clear();
            uiElementCache.clear();
        }

        #region 这一坨先留着，可能以后有用
        /// <summary>
        /// 第一个元素最开始的位置，用来做reset
        /// </summary>
        //private Vector2 originPos = Vector2.zero;

        /// <summary>
        /// 尝试初始化originPos
        /// </summary>
        /// <returns>初始化失败返回false，其他情况为true</returns>
        //bool tryInitOriginPos() {
        //    if(originPos != Vector2.zero) { return true; }
        //    if (uiElements.Count == 0) { return false; }
        //    if (originPos == Vector2.zero) {
        //        originPos = uiElements.First.Value.rectTransform.anchoredPosition;
        //        return true;
        //    }
        //    return false;
        //}

        /// <summary>
        /// 在LayoutGruop重新激活的时候(OnEnable); 
        /// 如果数据集没有发生变化，需要调用此方法，重新校正一下虚拟列表; 
        /// 如果数据集发生变化了，直接用reset，然后重新addItem吧; 
        /// </summary>
        //public void reShow() {
        //    //重置部分成员
        //    layoutGroupRectTrans.anchoredPosition = Vector2.zero;
        //    rearUIObjIndex = MaxElementCount - 1;
        //    lastLineCount = 0;
        //    uiElements.Clear();

        //    // 有坑！LayoutGroup的元素顺序是根据Transfrom的顺序去排列的
        //    // 所以我们要获取一下LayoutGroup里ui列表元素的Transform
        //    // 用它的顺序来重置uiElements
        //    List<RectTransform> tempList = new List<RectTransform>();
        //    foreach (RectTransform childTrans in layoutGroupRectTrans) {
        //        if (!childTrans.gameObject.activeInHierarchy) { continue; }
        //        tempList.Add(childTrans);
        //    }

        //    for (int elementIndex = 0; elementIndex < tempList.Count; elementIndex++) {
        //        UIElement element = new UIElement { rectTransform = tempList[elementIndex], uiObj = null };
        //        element.uiObj = element.rectTransform.gameObject as T;
        //        if (element.uiObj == null) { element.uiObj = element.rectTransform.GetComponent<T>(); }

        //        element.rectTransform.anchoredPosition = Vector2.zero;

        //        //uiElements.AddLast(element);
        //        onDisplayUIObj?.Invoke(elementIndex, element.uiObj);
        //    }
        //}
        #endregion
    }
}
