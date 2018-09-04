using System;
using UnityEngine;

namespace TBSGameCore.TriggerSystem
{
    abstract class TriggerExprDrawer : TriggerObjectDrawer
    {
        /// <summary>
        /// 绘制的目标的类型
        /// </summary>
        protected Type targetType { get; private set; }
        /// <summary>
        /// 绘制的目标的名字
        /// </summary>
        protected string targetName { get; private set; }
        public TriggerExprDrawer(Component targetObject, Transform transform, Type targetType, string targetName) : base(targetObject, transform)
        {
            this.targetType = targetType;
            this.targetName = targetName;
        }
        public TriggerExprDrawer(TriggerObjectDrawer parent, Transform transform, Type targetType, string targetName) : base(parent, transform)
        {
            this.targetType = targetType;
            this.targetName = targetName;
        }
    }
}