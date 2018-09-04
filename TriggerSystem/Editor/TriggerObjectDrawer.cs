using System.Linq;
using UnityEngine;

namespace TBSGameCore.TriggerSystem
{
    abstract class TriggerObjectDrawer
    {
        public abstract float height
        {
            get;
        }
        protected Component targetObject
        {
            get { return parent != null ? parent.targetObject : _targetObject; }
        }
        private Component _targetObject;
        protected Transform transform
        {
            get; private set;
        }
        protected TriggerMethodDefine[] funcLibrary
        {
            get { return parent != null ? parent.funcLibrary : _funcLibrary; }
        }
        TriggerMethodDefine[] _funcLibrary;
        TriggerObjectDrawer parent { get; set; } = null;
        public TriggerObjectDrawer(Component targetObject, Transform transform)
        {
            _targetObject = targetObject;
            this.transform = transform;
            if (!TriggerLibrary.isAssemblyLoaded(targetObject.GetType().Assembly))
                TriggerLibrary.load(targetObject.GetType().Assembly);
            _funcLibrary = TriggerLibrary.getMethodDefines();
        }
        public TriggerObjectDrawer(TriggerObjectDrawer parent, Transform transform)
        {
            this.parent = parent;
            this.transform = transform;
        }
    }
}