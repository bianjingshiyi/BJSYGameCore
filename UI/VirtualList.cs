using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace BJSYGameCore.UI
{


    public enum LayoutGroupType
    {
        Gird,
        Horizontal,
        Vertical,
    }
    public class VirtualList<T> where T : UIObject
    {
        public event Action<int,T> onDisPlayItem;
        
        private LayoutGroupType layoutGroupType;
        private Func<T> itemGernerator;
        
        private int frontIndex  = 0;
        private int backIndex = -1;

        public int HorizontalCount { get; set; }
        public int VerticalCount { get; set; }

        public int TotalCount { get { return HorizontalCount* VerticalCount; } }

        private List<T> realItems;

        private Vector2 oldScrollRectVal = Vector2.zero;

        public VirtualList(Func<T> itemGernerator, LayoutGroup layoutGroup)
        {
            realItems = new List<T>();
            ScrollRect scrollRect = layoutGroup.transform.GetComponentInParent<ScrollRect>();
            Rect viewPortRect = scrollRect.GetComponent<RectTransform>().rect;
            float realHeight = viewPortRect.height - layoutGroup.padding.top - layoutGroup.padding.bottom;
            float realWidth = viewPortRect.width - layoutGroup.padding.left - layoutGroup.padding.right;
            if(layoutGroup is GridLayoutGroup)
            {
                layoutGroupType = LayoutGroupType.Gird;
                GridLayoutGroup gridLayoutGroup = layoutGroup as GridLayoutGroup;
                HorizontalCount = (int)((realWidth + gridLayoutGroup.spacing.x) / (gridLayoutGroup.cellSize.x + gridLayoutGroup.spacing.x));
                VerticalCount =2+(int)((realHeight + gridLayoutGroup.spacing.y) / (gridLayoutGroup.cellSize.y + gridLayoutGroup.spacing.y));
            }
            if(layoutGroup is VerticalLayoutGroup)
            {
                layoutGroupType = LayoutGroupType.Vertical;
                VerticalLayoutGroup verticalLayoutGroup = layoutGroup as VerticalLayoutGroup;
                HorizontalCount = 1;
                VerticalCount =2 + (int)((realHeight + verticalLayoutGroup.spacing) /
                    (verticalLayoutGroup.transform.GetChild(1).GetComponent<RectTransform>().rect.height + verticalLayoutGroup.spacing));
            }
            if(layoutGroup is HorizontalLayoutGroup)
            {
                layoutGroupType = LayoutGroupType.Horizontal;
                HorizontalLayoutGroup horizontalLayoutGroup = layoutGroup as HorizontalLayoutGroup;
                VerticalCount = 1;
                HorizontalCount = 2 + (int)((realWidth + horizontalLayoutGroup.spacing) /
                    (horizontalLayoutGroup.transform.GetChild(1).GetComponent<RectTransform>().rect.width + horizontalLayoutGroup.spacing));
            }
            Debug.Log($"{HorizontalCount}**{VerticalCount}");
            
            scrollRect.onValueChanged.AddListener(onPlayerDrag);
            this.itemGernerator = itemGernerator;
        }

        public T addItem()
        {
            if (realItems.Count < TotalCount)
            {
                var item  = itemGernerator?.Invoke();
                realItems.Add(item as T);
                backIndex += 1;
                onDisPlayItem?.Invoke(backIndex,realItems[backIndex]);
                return item as T;
            }
            return null;
        }

        void onPlayerDrag(Vector2 val)
        {
            if(oldScrollRectVal == Vector2.zero)
            {
                oldScrollRectVal = val;
                return;
            }
            else
            {
                //列表向下滑动
                if (oldScrollRectVal.x < val.x)
                {

                }
                else // 列表向上滑动
                {

                }
            }
            Vector3 firstItemPos = realItems[0].transform.localPosition;
            switch (layoutGroupType)
            {
                //case LayoutGroupType.Gird:
                //    if (firstItemPos.x +)
            }

            Debug.Log($"{firstItemPos.x}TTT{firstItemPos.y}");
            Debug.Log($"{val.x}|||{val.y}");
        }
    }

}
