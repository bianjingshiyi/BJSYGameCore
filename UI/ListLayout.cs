using UnityEngine;
using UnityEngine.UI;

namespace BJSYGameCore.UI
{
    public class ListLayout : LayoutGroup
    {
        [SerializeField]
        float _spacing = 0;
        public float spacing
        {
            get { return _spacing; }
            set { SetProperty(ref _spacing, value); }
        }

        [SerializeField] protected bool m_ChildForceExpandWidth = true;

        /// <summary>
        /// Whether to force the children to expand to fill additional available horizontal space.
        /// </summary>
        public bool childForceExpandWidth { get { return m_ChildForceExpandWidth; } set { SetProperty(ref m_ChildForceExpandWidth, value); } }

        [SerializeField] protected bool m_ChildForceExpandHeight = true;

        /// <summary>
        /// Whether to force the children to expand to fill additional available vertical space.
        /// </summary>
        public bool childForceExpandHeight { get { return m_ChildForceExpandHeight; } set { SetProperty(ref m_ChildForceExpandHeight, value); } }

        [SerializeField] protected bool m_ChildControlWidth = true;

        /// <summary>
        /// Returns true if the Layout Group controls the widths of its children. Returns false if children control their own widths.
        /// </summary>
        /// <remarks>
        /// If set to false, the layout group will only affect the positions of the children while leaving the widths untouched. The widths of the children can be set via the respective RectTransforms in this case.
        ///
        /// If set to true, the widths of the children are automatically driven by the layout group according to their respective minimum, preferred, and flexible widths. This is useful if the widths of the children should change depending on how much space is available.In this case the width of each child cannot be set manually in the RectTransform, but the minimum, preferred and flexible width for each child can be controlled by adding a LayoutElement component to it.
        /// </remarks>
        public bool childControlWidth { get { return m_ChildControlWidth; } set { SetProperty(ref m_ChildControlWidth, value); } }

        [SerializeField] protected bool m_ChildControlHeight = true;

        /// <summary>
        /// Returns true if the Layout Group controls the heights of its children. Returns false if children control their own heights.
        /// </summary>
        /// <remarks>
        /// If set to false, the layout group will only affect the positions of the children while leaving the heights untouched. The heights of the children can be set via the respective RectTransforms in this case.
        ///
        /// If set to true, the heights of the children are automatically driven by the layout group according to their respective minimum, preferred, and flexible heights. This is useful if the heights of the children should change depending on how much space is available.In this case the height of each child cannot be set manually in the RectTransform, but the minimum, preferred and flexible height for each child can be controlled by adding a LayoutElement component to it.
        /// </remarks>
        public bool childControlHeight { get { return m_ChildControlHeight; } set { SetProperty(ref m_ChildControlHeight, value); } }
        [SerializeField]
        protected bool m_ChildScaleWidth = false;
        /// <summary>
        /// Whether to use the x scale of each child when calculating its width.
        /// </summary>
        public bool childScaleWidth
        {
            get { return m_ChildScaleWidth; }
            set { SetProperty(ref m_ChildScaleWidth, value); }
        }
        [SerializeField]
        protected bool m_ChildScaleHeight = false;
        /// <summary>
        /// Whether to use the y scale of each child when calculating its height.
        /// </summary>
        public bool childScaleHeight
        {
            get { return m_ChildScaleHeight; }
            set { SetProperty(ref m_ChildScaleHeight, value); }
        }
        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            CalcAlongAxis(0, false);
        }
        public override void CalculateLayoutInputVertical()
        {
            CalcAlongAxis(1, false);
        }
        public override void SetLayoutHorizontal()
        {
            SetChildrenAlongAxis(0, false);
        }
        public override void SetLayoutVertical()
        {
            SetChildrenAlongAxis(1, false);
        }
        /// <summary>
        /// Calculate the layout element properties for this layout element along the given axis.
        /// </summary>
        /// <param name="axis">The axis to calculate for. 0 is horizontal and 1 is vertical.</param>
        /// <param name="isVertical">Is this group a vertical group?</param>
        protected void CalcAlongAxis(int axis, bool isVertical)
        {
            float combinedPadding = (axis == 0 ? padding.horizontal : padding.vertical);//边缘总宽度
            bool controlChildSize = (axis == 0 ? childControlWidth : childControlHeight);//控制子物体大小
            bool useScale = (axis == 0 ? childScaleWidth : childScaleHeight);//计算子物体的缩放
            bool childForceExpandSize = (axis == 0 ? childForceExpandWidth : childForceExpandHeight);//强制展开子物体

            float totalMin = combinedPadding;
            float totalPreferred = combinedPadding;
            float totalFlexible = 0;

            bool alongOtherAxis = (isVertical ^ (axis == 1));//是不是在计算其他轴上的宽度
            for (int i = 0; i < rectChildren.Count; i++)
            {
                RectTransform child = rectChildren[i];
                GetChildSizes(child, axis, controlChildSize, childForceExpandSize, out float min, out float preferred, out float flexible);

                if (useScale)
                {
                    float scaleFactor = child.localScale[axis];
                    min *= scaleFactor;
                    preferred *= scaleFactor;
                    flexible *= scaleFactor;
                }

                if (alongOtherAxis)
                {
                    //如果在计算其他轴上的宽度，就只需要返回最宽的那个就行了
                    totalMin = Mathf.Max(min + combinedPadding, totalMin);
                    totalPreferred = Mathf.Max(preferred + combinedPadding, totalPreferred);
                    totalFlexible = Mathf.Max(flexible, totalFlexible);
                }
                else
                {
                    //不是，累加最小和合适宽度
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
                float maxSize = rectTransform.rect.size[axis];
                totalMin = Mathf.Min(totalMin, maxSize);
                totalPreferred = Mathf.Min(totalPreferred, maxSize);
            }
            totalPreferred = Mathf.Max(totalMin, totalPreferred);
            SetLayoutInputForAxis(totalMin, totalPreferred, totalFlexible, axis);
        }
        /// <summary>
        /// Set the positions and sizes of the child layout elements for the given axis.
        /// </summary>
        /// <param name="axis">The axis to handle. 0 is horizontal and 1 is vertical.</param>
        /// <param name="isVertical">Is this group a vertical group?</param>
        protected void SetChildrenAlongAxis(int axis, bool isVertical)
        {
            float size = rectTransform.rect.size[axis];
            bool controlChildSize = (axis == 0 ? m_ChildControlWidth : m_ChildControlHeight);
            bool useScale = (axis == 0 ? m_ChildScaleWidth : m_ChildScaleHeight);
            bool childForceExpandSize = (axis == 0 ? m_ChildForceExpandWidth : m_ChildForceExpandHeight);
            float alignmentOnAxis = GetAlignmentOnAxis(axis);

            bool alongOtherAxis = (isVertical ^ (axis == 1));//轴与方向是否一致？
            if (alongOtherAxis)
            {
                //计算非排列轴
                float innerSize = size - (axis == 0 ? padding.horizontal : padding.vertical);//布局内部空间大小
                for (int i = 0; i < rectChildren.Count; i++)
                {
                    RectTransform child = rectChildren[i];
                    GetChildSizes(child, axis, controlChildSize, childForceExpandSize, out float min, out float preferred, out float flexible);
                    float scaleFactor = useScale ? child.localScale[axis] : 1f;

                    float requiredSpace = Mathf.Clamp(innerSize, min, flexible > 0 ? size : preferred);
                    float startOffset = GetStartOffset(axis, requiredSpace * scaleFactor);
                    if (controlChildSize)
                    {
                        SetChildAlongAxisWithScale(child, axis, startOffset, requiredSpace, scaleFactor);
                    }
                    else
                    {
                        float offsetInCell = (requiredSpace - child.sizeDelta[axis]) * alignmentOnAxis;
                        SetChildAlongAxisWithScale(child, axis, startOffset + offsetInCell, scaleFactor);
                    }
                }
            }
            else
            {
                float pos = axis == 0 ? padding.left : padding.top;
                if (GetTotalPreferredSize(axis) > size)
                {
                    RectTransform child = rectChildren[rectChildren.Count - 1];
                    GetChildSizes(child, axis, controlChildSize, childForceExpandSize, out float min, out float preferred, out float flexible);
                    float childSize = preferred;
                    float remainedSpace = size - childSize;
                    for (int i = 0; i < rectChildren.Count; i++)
                    {
                        child = rectChildren[i];
                        GetChildSizes(child, axis, controlChildSize, childForceExpandSize, out min, out preferred, out flexible);
                        float scaleFactor = useScale ? child.localScale[axis] : 1f;
                        childSize = preferred;

                        if (controlChildSize)
                        {
                            SetChildAlongAxisWithScale(child, axis, pos, childSize, scaleFactor);
                        }
                        else
                        {
                            SetChildAlongAxisWithScale(child,axis,pos,)
                        }
                    }
                }
                else
                {
                    float itemFlexibleMultiplier = 0;
                    float surplusSpace = size - GetTotalPreferredSize(axis);//多余的空间

                    if (surplusSpace > 0)
                    {
                        if (GetTotalFlexibleSize(axis) == 0)
                            pos = GetStartOffset(axis, GetTotalPreferredSize(axis) - (axis == 0 ? padding.horizontal : padding.vertical));
                        else if (GetTotalFlexibleSize(axis) > 0)
                            itemFlexibleMultiplier = surplusSpace / GetTotalFlexibleSize(axis);
                    }

                    float minMaxLerp = 0;
                    if (GetTotalMinSize(axis) != GetTotalPreferredSize(axis))
                        minMaxLerp = Mathf.Clamp01((size - GetTotalMinSize(axis)) / (GetTotalPreferredSize(axis) - GetTotalMinSize(axis)));

                    for (int i = 0; i < rectChildren.Count; i++)
                    {
                        RectTransform child = rectChildren[i];
                        GetChildSizes(child, axis, controlChildSize, childForceExpandSize, out float min, out float preferred, out float flexible);
                        float scaleFactor = useScale ? child.localScale[axis] : 1f;

                        float childSize = Mathf.Lerp(min, preferred, minMaxLerp);
                        childSize += flexible * itemFlexibleMultiplier;
                        if (controlChildSize)
                        {
                            SetChildAlongAxisWithScale(child, axis, pos, childSize, scaleFactor);
                        }
                        else
                        {
                            float offsetInCell = (childSize - child.sizeDelta[axis]) * alignmentOnAxis;
                            SetChildAlongAxisWithScale(child, axis, pos + offsetInCell, scaleFactor);
                        }
                        pos += childSize * scaleFactor + spacing;
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="child"></param>
        /// <param name="axis"></param>
        /// <param name="controlChildSize">如果为真，采用子物体在布局上的尺寸（包括LayoutElement等），如果为假，直接使用子物体Transform的尺寸。</param>
        /// <param name="childForceExpand"></param>
        /// <param name="min"></param>
        /// <param name="preferred"></param>
        /// <param name="flexible"></param>
        private void GetChildSizes(RectTransform child, int axis, bool controlChildSize, bool childForceExpand, out float min, out float preferred, out float flexible)
        {
            if (!controlChildSize)
            {
                min = child.sizeDelta[axis];
                preferred = min;
                flexible = 0;
            }
            else
            {
                min = LayoutUtility.GetMinSize(child, axis);
                preferred = LayoutUtility.GetPreferredSize(child, axis);
                flexible = LayoutUtility.GetFlexibleSize(child, axis);
            }

            if (childForceExpand)
                flexible = Mathf.Max(flexible, 1);
        }
#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();

            // For new added components we want these to be set to false,
            // so that the user's sizes won't be overwritten before they
            // have a chance to turn these settings off.
            // However, for existing components that were added before this
            // feature was introduced, we want it to be on be default for
            // backwardds compatibility.
            // Hence their default value is on, but we set to off in reset.
            m_ChildControlWidth = false;
            m_ChildControlHeight = false;
        }

        private int m_Capacity = 10;
        private Vector2[] m_Sizes = new Vector2[10];

        protected virtual void Update()
        {
            if (Application.isPlaying)
                return;

            int count = transform.childCount;

            if (count > m_Capacity)
            {
                if (count > m_Capacity * 2)
                    m_Capacity = count;
                else
                    m_Capacity *= 2;

                m_Sizes = new Vector2[m_Capacity];
            }

            // If children size change in editor, update layout (case 945680 - Child GameObjects in a Horizontal/Vertical Layout Group don't display their correct position in the Editor)
            bool dirty = false;
            for (int i = 0; i < count; i++)
            {
                RectTransform t = transform.GetChild(i) as RectTransform;
                if (t != null && t.sizeDelta != m_Sizes[i])
                {
                    dirty = true;
                    m_Sizes[i] = t.sizeDelta;
                }
            }

            if (dirty)
                LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
        }
#endif
    }
}