using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BJSYGameCore.UI
{
    public class ListLayoutGroup : LayoutGroup
    {
        [SerializeField]
        bool _averageSpacing = false;
        public bool averageSpaceing
        {
            get { return _averageSpacing; }
            set { SetProperty(ref _averageSpacing, value); }
        }
        [SerializeField]
        float _spacing = 0;
        public float spacing
        {
            get { return _spacing; }
            set { SetProperty(ref _spacing, value); }
        }
        [SerializeField]
        bool _forceChildExpandWidth = false;
        public bool forceChildExpandWidth
        {
            get { return _forceChildExpandWidth; }
            set { SetProperty(ref _forceChildExpandWidth, value); }
        }
        [SerializeField]
        bool _forceChildExpandHeight = false;
        public bool forceChildExpandHeight
        {
            get { return _forceChildExpandHeight; }
            set { SetProperty(ref _forceChildExpandHeight, value); }
        }
        [Tooltip("在空间足以容纳所有内容，并且内容倾向于占满整个空间的时候，如果空间大于该尺寸，那么空间会倾向于缩小到该尺寸。")]
        [SerializeField]
        Vector2 _preferredSize;
        public Vector2 preferredSize
        {
            get { return _preferredSize; }
            set { SetProperty(ref _preferredSize, value); }
        }
        [SerializeField]
        OverflowType _overflowType;
        public OverflowType overflowType
        {
            get { return _overflowType; }
            set { SetProperty(ref _overflowType, value); }
        }
        public enum OverflowType
        {
            extrusion,
            overflow
        }
        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            calcAlongAxis(0, false);
        }
        public override void CalculateLayoutInputVertical()
        {
            calcAlongAxis(1, false);
        }
        public override void SetLayoutHorizontal()
        {
            setChildrenAlongAxis(0, false);
        }
        public override void SetLayoutVertical()
        {
            setChildrenAlongAxis(1, false);
        }
        void calcAlongAxis(int axis, bool isVertical)
        {
            float totalPadding = axis == 0 ? padding.horizontal : padding.vertical;
            float totalMin = totalPadding;
            float totalPreferred = totalPadding;
            float totalFlexible = 0;
            bool forceChildExpand = axis == 0 ? forceChildExpandWidth : forceChildExpandHeight;

            bool alongOtherAxis = (isVertical ^ (axis == 1));
            for (int i = 0; i < rectChildren.Count; i++)
            {
                RectTransform child = rectChildren[i];
                float min = child.rect.size[axis];
                float preferred = min;
                float flexible = 0;

                float scaleFactor = child.localScale[axis];
                min *= scaleFactor;
                preferred *= scaleFactor;
                flexible *= scaleFactor;

                if (alongOtherAxis)
                {
                    totalMin = Mathf.Max(min + totalPadding, totalMin);
                    totalPreferred = Mathf.Max(preferred + totalPadding, totalPreferred);
                    totalFlexible = Mathf.Max(flexible, totalFlexible);
                }
                else
                {
                    totalMin += min + spacing;
                    totalPreferred += preferred + spacing;
                    // Increment flexible size with element's flexible size.
                    totalFlexible += flexible;
                }
            }
            if (!alongOtherAxis && rectChildren.Count > 0)
            {
                totalMin -= spacing;
                totalPreferred -= spacing;
                float currentSize = rectTransform.rect.size[axis];
                if (totalPreferred < currentSize)//空间充足
                {
                    if (currentSize > preferredSize[axis])
                        currentSize = preferredSize[axis];
                    if (averageSpaceing)
                        totalPreferred = currentSize;
                }
                else if (totalPreferred > currentSize)//空间不足
                {
                    if (overflowType == OverflowType.extrusion)
                        totalPreferred = currentSize;
                }
            }
            else if (alongOtherAxis)
            {
                if (forceChildExpand)
                    totalPreferred = rectTransform.rect.size[axis];
            }
            totalPreferred = Mathf.Max(totalMin, totalPreferred);
            SetLayoutInputForAxis(totalMin, totalPreferred, totalFlexible, axis);
        }
        void setChildrenAlongAxis(int axis, bool isVertical)
        {
            float innerSize = rectTransform.rect.size[axis] - (axis == 0 ? padding.horizontal : padding.vertical);
            float alignmentOnAxis = GetAlignmentOnAxis(axis);
            bool alongOtherAxis = (isVertical ^ (axis == 1));
            bool forceChildExpand = axis == 0 ? forceChildExpandWidth : forceChildExpandHeight;

            if (alongOtherAxis)
            {
                for (int i = 0; i < rectChildren.Count; i++)
                {
                    RectTransform child = rectChildren[i];
                    if (forceChildExpand)
                    {
                        float childSize = rectTransform.rect.size[axis];
                        float scaleFactor = 1;
                        float startOffset = GetStartOffset(axis, childSize * scaleFactor);
                        SetChildAlongAxisWithScale(child, axis, startOffset, childSize, scaleFactor);
                    }
                    else
                    {
                        float childSize = child.rect.size[axis];
                        float scaleFactor = child.localScale[axis];
                        float startOffset = GetStartOffset(axis, childSize * scaleFactor);
                        SetChildAlongAxisWithScale(child, axis, startOffset, scaleFactor);
                    }
                }
            }
            else
            {
                float expandedSize = rectChildren.Sum(child => child.rect.size[axis] * child.localScale[axis]) + spacing * (rectChildren.Count - 1);
                if (expandedSize > innerSize)
                {
                    if (overflowType == OverflowType.extrusion)
                    {
                        float pos = axis == 0 ? padding.left : padding.top;
                        if (rectChildren.Count < 1)
                            return;
                        RectTransform child = rectChildren[rectChildren.Count - 1];
                        float childSize = child.rect.size[axis] * child.localScale[axis];
                        float remainedSpace = innerSize - childSize;
                        float remainedExpandedSize = expandedSize - childSize;
                        for (int i = 0; i < rectChildren.Count; i++)
                        {
                            child = rectChildren[i];
                            childSize = child.rect.size[axis];
                            float scaleFactor = child.localScale[axis];
                            SetChildAlongAxisWithScale(child, axis, pos, scaleFactor);
                            pos += remainedSpace / remainedExpandedSize * (childSize * scaleFactor + spacing);
                        }
                    }
                    else
                    {
                        float pos = GetStartOffset(axis, GetTotalPreferredSize(axis) - (axis == 0 ? padding.horizontal : padding.vertical));
                        for (int i = 0; i < rectChildren.Count; i++)
                        {
                            RectTransform child = rectChildren[i];
                            float childSize = child.rect.size[axis];
                            float scaleFactor = child.localScale[axis];
                            SetChildAlongAxisWithScale(child, axis, pos, scaleFactor);
                            pos += spacing + childSize * scaleFactor;
                        }
                    }
                }
                else
                {
                    if (expandedSize < innerSize && averageSpaceing)
                    {
                        float totalSize = rectChildren.Sum(child => child.rect.size[axis] * child.localScale[axis]);
                        float remainedSpace = innerSize - totalSize;
                        float averageSpacing = remainedSpace / (rectChildren.Count + 1);
                        float pos = padding.left;
                        for (int i = 0; i < rectChildren.Count; i++)
                        {
                            RectTransform child = rectChildren[i];
                            float childSize = child.rect.size[axis];
                            float scaleFactor = child.localScale[axis];
                            pos += averageSpacing;
                            SetChildAlongAxisWithScale(child, axis, pos, scaleFactor);
                            pos += childSize * scaleFactor;
                        }
                    }
                    else
                    {
                        float pos = GetStartOffset(axis, GetTotalPreferredSize(axis) - (axis == 0 ? padding.horizontal : padding.vertical));
                        for (int i = 0; i < rectChildren.Count; i++)
                        {
                            RectTransform child = rectChildren[i];
                            float childSize = child.rect.size[axis];
                            float scaleFactor = child.localScale[axis];
                            SetChildAlongAxisWithScale(child, axis, pos, scaleFactor);
                            pos += spacing + childSize * scaleFactor;
                        }
                    }
                }
            }
        }
#if UNITY_EDITOR
        protected override void Reset()
        {
            preferredSize = rectTransform.rect.size;
        }
#endif
    }
}