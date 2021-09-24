using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using static UnityEngine.UI.GridLayoutGroup;
using System;

namespace BJSYGameCore.UI
{
    /// <summary>
    /// 虚拟列表布局，当一个滑动列表中要显示非常多的元素的时候，可以用这个组件来优化性能。
    /// 虚拟列表布局使用网格自动布局，必须指定网格的大小和间隔，在计算它的大小时会计算所有列表项的大小。
    /// 必须位于一个ScrollRect的Content上，虚拟列表只会创建能在视口中显示的UI物体，在有数千个需要显示的项目的情况下，只会创建能被看到的数十个项目。
    /// 当项目变得不可见的时候，会将项目禁用并放入对象池，如果对象池中已经有物体，会从对象池中取出已有物体。
    /// 虚拟列表布局重写Unity提供的布局组件，这是为了更好的配合内容大小自适应。
    /// </summary>
    public class VirtualListLayoutGroup : LayoutGroup
    {
        #region 公有方法
        /// <summary>
        /// 设置虚拟列表显示的单元格总数量，但是只有能被看到的单元格会被创建和显示。
        /// 会在下一次LateUpdate的时候触发onEnableItem和DisableItem事件。
        /// </summary>
        /// <param name="count"></param>
        public void setCount(int count)
        {
            if (count < 0)
                throw new ArgumentException("Param count can not be less than zero", nameof(count));
            totalCount = count;
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }
        /// <summary>
        /// 获取一个单元格RectTransform对应的索引。这个索引是相对总体数量而言的。
        /// 这个RectTransform必须是当前正在显示的单元格，由于单元格会被回收和重新初始化，所以一个RectTransform对应的索引并不是不变的。
        /// 如果该物体当前并未被显示，返回的索引为-1。
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int getItemIndex(RectTransform item)
        {
            if (_startIndex < 0)
                return -1;
            int index = _childList.IndexOf(item);
            if (index < 0)
                return -1;
            return _startIndex + index;
        }
        /// <summary>
        /// 调用布局系统来计算横向布局大小。
        /// </summary>
        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            int minColumns;
            int preferredColumns;
            if (constraint == Constraint.FixedColumnCount)
            {
                //约束列数，最小和最适列数均为指定列数
                minColumns = preferredColumns = constraintCount;
            }
            else if (constraint == Constraint.FixedRowCount)
            {
                //约束行数，最小和最适列数均通过指定行数除以实际
                minColumns = preferredColumns = Mathf.CeilToInt(totalCount / (float)constraintCount - 0.001f);
            }
            else
            {
                //没有任何约束，最小列数为1，最适列数为总数量的开方
                minColumns = 1;
                preferredColumns = Mathf.CeilToInt(Mathf.Sqrt(totalCount));
            }

            SetLayoutInputForAxis(
                padding.horizontal + (cellSize.x + spacing.x) * minColumns - spacing.x,
                padding.horizontal + (cellSize.x + spacing.x) * preferredColumns - spacing.x,
                -1, 0);
        }
        public override void CalculateLayoutInputVertical()
        {
            int minRows;
            if (constraint == Constraint.FixedColumnCount)
            {
                //约束列数，最小行数为总数除以约束列数。
                minRows = Mathf.CeilToInt(totalCount / (float)constraintCount - 0.001f);
            }
            else if (constraint == Constraint.FixedRowCount)
            {
                //约束行数，最小行数为指定行数
                minRows = constraintCount;
            }
            else
            {
                //无约束，行数为当前矩形变换宽度能容忍的最小行数
                float width = rectTransform.rect.width;
                int cellCountX = Mathf.Max(1, Mathf.FloorToInt((width - padding.horizontal + spacing.x + 0.001f) / (cellSize.x + spacing.x)));
                minRows = Mathf.CeilToInt(totalCount / (float)cellCountX);
            }

            float minSpace = padding.vertical + (cellSize.y + spacing.y) * minRows - spacing.y;
            SetLayoutInputForAxis(minSpace, minSpace, -1, 1);
        }
        public override void SetLayoutHorizontal()
        {
            SetCellsAlongAxis(0);
        }
        public override void SetLayoutVertical()
        {
            SetCellsAlongAxis(1);
        }
        protected override void Awake()
        {
            base.Awake();
            _scrollRect = GetComponentInParent<ScrollRect>();
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            if (_scrollRect != null)
            {
                if (_scrollRect.verticalScrollbar != null)
                {
                    _scrollRect.verticalScrollbar.onValueChanged.RemoveListener(onBarScroll);
                    _scrollRect.verticalScrollbar.onValueChanged.AddListener(onBarScroll);
                }
                if (_scrollRect.horizontalScrollbar != null)
                {
                    _scrollRect.horizontalScrollbar.onValueChanged.RemoveListener(onBarScroll);
                    _scrollRect.horizontalScrollbar.onValueChanged.AddListener(onBarScroll);
                }
            }
            if (_startIndex >= 0)
            {
                for (int i = 0; i < _childList.Count; i++)
                {
                    onEnableItem?.Invoke(_startIndex + i, _childList[i]);
                }
            }
        }
        protected void LateUpdate()
        {
            if (_disableChildList.Count > 0)
            {
                for (int i = 0; i < _disableChildList.Count; i++)
                {
                    onDisableItem?.Invoke(_disableChildList[i].Item1, _disableChildList[i].Item2);
                    _disableChildList[i].Item2.gameObject.SetActive(false);
                }
                _disableChildList.Clear();
            }
            if (_enableChildList.Count > 0)
            {
                for (int i = 0; i < _enableChildList.Count; i++)
                {
                    onEnableItem?.Invoke(_enableChildList[i].Item1, _enableChildList[i].Item2);
                    _enableChildList[i].Item2.gameObject.SetActive(true);
                }
                _enableChildList.Clear();
            }
            if (_updateChildList.Count > 0)
            {
                for (int i = 0; i < _updateChildList.Count; i++)
                {
                    onUpdateItem?.Invoke(_updateChildList[i].Item1, _updateChildList[i].Item2);
                }
                _updateChildList.Clear();
            }
        }
        #endregion
        #region  私有方法
        private void SetCellsAlongAxis(int axis)
        {
            if (_scrollRect == null)
            {
                _scrollRect = GetComponentInParent<ScrollRect>();
                if (_scrollRect != null)
                {
                    _scrollRect.verticalScrollbar.onValueChanged.RemoveListener(onBarScroll);
                    _scrollRect.verticalScrollbar.onValueChanged.AddListener(onBarScroll);
                    _scrollRect.horizontalScrollbar.onValueChanged.RemoveListener(onBarScroll);
                    _scrollRect.horizontalScrollbar.onValueChanged.AddListener(onBarScroll);
                }
            }
            if (_scrollRect == null)
                return;
            if (cellPrefab == null)
                return;

            //一般一个布局控制器在横向轴调用时应该只设置横向值，在纵向轴调用时应该只设置纵向值。
            //然而，在这里我们在纵向轴调用的时候同时设置横向轴和纵向轴的位置。
            //由于我们只设置横向位置而不是大小，这不会影响到子物体的布局。
            //所以这不会破坏所有横向布局应该在纵向布局之前计算的规则。

            if (axis == 0)
            {
                //在横向轴调用的时候只设置大小而非位置。
                for (int i = 0; i < _childList.Count; i++)
                {
                    RectTransform rect = _childList[i];
                    if (rect == null)
                    {
                        _childList.RemoveAt(i);
                        i--;
                        continue;
                    }
                    m_Tracker.Add(this, rect,
                        DrivenTransformProperties.Anchors |
                        DrivenTransformProperties.AnchoredPosition |
                        DrivenTransformProperties.SizeDelta);

                    rect.anchorMin = Vector2.up;
                    rect.anchorMax = Vector2.up;
                    rect.sizeDelta = cellSize;
                }
                return;
            }

            float width = rectTransform.rect.size.x;
            float height = rectTransform.rect.size.y;
            //计算XY轴单元数量
            int cellCountX = 1;
            int cellCountY = 1;
            if (constraint == Constraint.FixedColumnCount)
            {
                //约束列数，计算行数
                cellCountX = constraintCount;
                if (totalCount > cellCountX)
                    cellCountY = totalCount / cellCountX + (totalCount % cellCountX > 0 ? 1 : 0);
            }
            else if (constraint == Constraint.FixedRowCount)
            {
                //约束行数，计算列数
                cellCountY = constraintCount;
                if (totalCount > cellCountY)
                    cellCountX = totalCount / cellCountY + (totalCount % cellCountY > 0 ? 1 : 0);
            }
            else
            {
                //没有约束，根据长款本身计算行列。
                if (cellSize.x + spacing.x <= 0)
                    cellCountX = int.MaxValue;
                else
                    cellCountX = Mathf.Max(1, Mathf.FloorToInt((width - padding.horizontal + spacing.x + 0.001f) / (cellSize.x + spacing.x)));

                if (cellSize.y + spacing.y <= 0)
                    cellCountY = int.MaxValue;
                else
                    cellCountY = Mathf.Max(1, Mathf.FloorToInt((height - padding.vertical + spacing.y + 0.001f) / (cellSize.y + spacing.y)));
            }
            //计算单元格数量
            int cornerX = (int)startCorner % 2;
            int cornerY = (int)startCorner / 2;
            int cellsPerMainAxis, actualCellCountX, actualCellCountY;
            if (startAxis == Axis.Horizontal)
            {
                cellsPerMainAxis = cellCountX;
                actualCellCountX = Mathf.Clamp(cellCountX, 1, totalCount);
                actualCellCountY = Mathf.Clamp(cellCountY, 1, Mathf.CeilToInt(totalCount / (float)cellsPerMainAxis));
            }
            else
            {
                cellsPerMainAxis = cellCountY;
                actualCellCountY = Mathf.Clamp(cellCountY, 1, totalCount);
                actualCellCountX = Mathf.Clamp(cellCountX, 1, Mathf.CeilToInt(totalCount / (float)cellsPerMainAxis));
            }
            //设置所有单元格物体坐标
            Vector2 requiredSpace = new Vector2(
                actualCellCountX * cellSize.x + (actualCellCountX - 1) * spacing.x,
                actualCellCountY * cellSize.y + (actualCellCountY - 1) * spacing.y);
            Vector2 startOffset = new Vector2(
                GetStartOffset(0, requiredSpace.x),
                GetStartOffset(1, requiredSpace.y));
            int viewportStartIndex = -1;
            for (int i = 0; i < totalCount; i++)
            {
                int positionX;
                int positionY;
                if (startAxis == Axis.Horizontal)
                {
                    positionX = i % cellsPerMainAxis;
                    positionY = i / cellsPerMainAxis;
                }
                else
                {
                    positionX = i / cellsPerMainAxis;
                    positionY = i % cellsPerMainAxis;
                }

                if (cornerX == 1)
                    positionX = actualCellCountX - 1 - positionX;
                if (cornerY == 1)
                    positionY = actualCellCountY - 1 - positionY;
                float posX = startOffset.x + (cellSize[0] + spacing[0]) * positionX;
                float posY = startOffset.y + (cellSize[1] + spacing[1]) * positionY;
                Rect rect = new Rect(
                    _scrollRect.viewport.InverseTransformPoint(rectTransform.TransformPoint(new Vector3(posX, -posY - cellSize[1]))),
                    cellSize);
                RectTransform child;
                //判断当前位置是否在视口中
                if (_scrollRect.viewport.rect.Overlaps(rect))
                {
                    if (viewportStartIndex < 0)
                        viewportStartIndex = i;
                    //尝试获取当前索引的单元的物体，如果不存在则新建
                    if (0 <= i - _startIndex && i - _startIndex < _childList.Count)
                    {
                        child = _childList[i - _startIndex];
                        if (child == null)
                        {
                            if (_poolList.Count < 1)
                            {
                                child = Instantiate(cellPrefab, transform);
                            }
                            else
                            {
                                child = _poolList[0];
                                _poolList.RemoveAt(0);
                            }
                            _childList[i - _startIndex] = child;
                            _enableChildList.Add(new Tuple<int, RectTransform>(i, child));
                        }
                    }
                    else if (i < _startIndex)//创建分为两种情况，后面缺和前面缺
                    {
                        //前面缺，需要改索引然后更改列表长度
                        RectTransform[] newChilds = new RectTransform[_startIndex - i];
                        if (_poolList.Count < 1)
                        {
                            child = Instantiate(cellPrefab, transform);
                        }
                        else
                        {
                            child = _poolList[0];
                            _poolList.RemoveAt(0);
                        }
                        newChilds[0] = child;
                        _childList.InsertRange(0, newChilds);
                        _startIndex = i;
                        _enableChildList.Add(new Tuple<int, RectTransform>(i, child));
                    }
                    else
                    {
                        //后面缺，不需要改索引，直接往后面加
                        if (_poolList.Count < 1)
                        {
                            child = Instantiate(cellPrefab, transform);
                        }
                        else
                        {
                            child = _poolList[0];
                            _poolList.RemoveAt(0);
                        }
                        _childList.Add(child);
                        _enableChildList.Add(new Tuple<int, RectTransform>(i, child));
                    }
                    SetChildAlongAxis(child, 0, posX, cellSize[0]);
                    SetChildAlongAxis(child, 1, posY, cellSize[1]);
                    _updateChildList.Add(new Tuple<int, RectTransform>(i, child));
                }
                else
                {
                    //不在视口中，尝试获取当前索引的单元物体
                    if (0 <= i - _startIndex && i - _startIndex < _childList.Count)
                    {
                        if (viewportStartIndex < 0)
                        {
                            //在视口前方，先往后挪
                            child = _childList[0];
                            _childList.RemoveAt(0);
                            _startIndex++;
                            if (child != null)
                            {
                                _poolList.Insert(0, child);
                                _disableChildList.Add(new Tuple<int, RectTransform>(i, child));
                            }
                        }
                        else
                        {
                            //在视口后方
                            child = _childList[i - _startIndex];
                            _childList.RemoveAt(i - _startIndex);
                            if (child != null)
                            {
                                _poolList.Insert(0, child);
                                _disableChildList.Add(new Tuple<int, RectTransform>(i, child));
                            }
                            i--;
                        }
                    }
                }
            }

            if (viewportStartIndex < 0)
            {
                //视口中没有任何可见单元，全部回收
                while (_childList.Count > 0)
                {
                    RectTransform child = _childList[_childList.Count - 1];
                    _childList.RemoveAt(_childList.Count - 1);
                    _poolList.Insert(0, child);
                    _disableChildList.Add(new Tuple<int, RectTransform>(_startIndex + _childList.Count, child));
                }
            }
            else
            {
                //回收超出当前数量的单元物体
                while (_startIndex + _childList.Count > totalCount)
                {
                    RectTransform child = _childList[_childList.Count - 1];
                    _childList.RemoveAt(_childList.Count - 1);
                    if (child != null)
                    {
                        //child.gameObject.SetActive(false);
                        _poolList.Insert(0, child);
                        _disableChildList.Add(new Tuple<int, RectTransform>(_startIndex + _childList.Count, child));
                    }
                }
            }
        }
        void onBarScroll(float value)
        {
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }
        #endregion
        /// <summary>
        /// 当单元格显示的时候触发，参数是显示单元格对应索引和单元格
        /// </summary>
        public event Action<int, RectTransform> onEnableItem;
        /// <summary>
        /// 当单元格需要更新的时候触发，参数是更新单元格对应索引和单元格
        /// </summary>
        public event Action<int, RectTransform> onUpdateItem;
        /// <summary>
        /// 当单元格隐藏的时候触发，参数是隐藏单元格对应索引和单元格。注意被回收的物体对应的索引是可能超出当前数量上限的。
        /// </summary>
        public event Action<int, RectTransform> onDisableItem;
        /// <summary>
        /// 单元应该被首先放在哪个角落？
        /// </summary>
        public Corner startCorner
        {
            get { return _startCorner; }
            set { SetProperty(ref _startCorner, value); }
        }
        [SerializeField]
        protected Corner _startCorner = Corner.UpperLeft;
        /// <summary>
        /// 单元应该被首先沿着哪个轴放置？
        /// </summary>
        /// <remarks>
        /// 当起始轴被设置为横向时，直到一整行被填满之前不会处理下一行。
        /// 当被设置为纵向时，直到一整列被填满之前不会处理下一列。
        /// </remarks>
        public Axis startAxis
        {
            get { return _startAxis; }
            set { SetProperty(ref _startAxis, value); }
        }
        [SerializeField]
        protected Axis _startAxis = Axis.Horizontal;
        /// <summary>
        /// 网格中的每一个单元的尺寸。
        /// </summary>
        public Vector2 cellSize
        {
            get { return _cellSize; }
            set { SetProperty(ref _cellSize, value); }
        }
        [SerializeField]
        protected Vector2 _cellSize = new Vector2(100, 100);
        /// <summary>
        /// 网格中两个元素在两个轴上的间隔。
        /// </summary>
        public Vector2 spacing
        {
            get { return _spacing; }
            set { SetProperty(ref _spacing, value); }
        }
        [SerializeField]
        protected Vector2 _spacing = Vector2.zero;
        /// <summary>
        /// 虚拟列表布局所使用的约束。
        /// </summary>
        /// <remarks>
        /// 指定一个约束可以让虚拟列表布局更好的和内容尺寸自适应工作。当虚拟列表布局被用于一个手动指定了大小的矩形变换时，不需要指定一个约束。
        /// </remarks>
        public Constraint constraint
        {
            get { return _constraint; }
            set { SetProperty(ref _constraint, value); }
        }
        [SerializeField]
        protected Constraint _constraint = Constraint.Flexible;
        /// <summary>
        /// 在被约束的轴上应该有多少个列表项。
        /// </summary>
        public int constraintCount
        {
            get { return _constraintCount; }
            set { SetProperty(ref _constraintCount, Mathf.Max(1, value)); }
        }
        [SerializeField]
        protected int _constraintCount = 2;
        /// <summary>
        /// 虚拟列表布局中物体的总数量，区别于实际上能看见的数量。
        /// </summary>
        public int totalCount { get; private set; }
        /// <summary>
        /// 单元预制件
        /// </summary>
        public RectTransform cellPrefab
        {
            get { return _cellPrefab; }
            set { SetProperty(ref _cellPrefab, value); }
        }
        [SerializeField]
        RectTransform _cellPrefab;
        /// <summary>
        /// 当前显示的单元格的起始索引
        /// </summary>
        int _startIndex = 0;
        /// <summary>
        /// 当前显示的所有单元格
        /// </summary>
        List<RectTransform> _childList = new List<RectTransform>();
        List<RectTransform> _poolList = new List<RectTransform>();
        /// <summary>
        /// 要激活的单元格列表，字段为单元格索引与单元格
        /// </summary>
        List<Tuple<int, RectTransform>> _enableChildList = new List<Tuple<int, RectTransform>>();
        List<Tuple<int, RectTransform>> _updateChildList = new List<Tuple<int, RectTransform>>();
        List<Tuple<int, RectTransform>> _disableChildList = new List<Tuple<int, RectTransform>>();
        ScrollRect _scrollRect;
    }
}
