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
        protected TriggerFuncDefine[] funcLibrary
        {
            get { return parent != null ? parent.funcLibrary : _funcLibrary; }
        }
        TriggerFuncDefine[] _funcLibrary;
        protected GUIContent[] funcOptions
        {
            get { return parent != null ? parent.funcOptions : _funcOptions; }
        }
        GUIContent[] _funcOptions;
        TriggerObjectDrawer parent { get; set; } = null;
        public TriggerObjectDrawer(Component targetObject, Transform transform)
        {
            _targetObject = targetObject;
            this.transform = transform;
            if (!TriggerLibrary.isAssemblyLoaded(targetObject.GetType().Assembly))
                TriggerLibrary.load(targetObject.GetType().Assembly);
            _funcLibrary = TriggerLibrary.getFuncDefines();
            _funcOptions = TriggerLibrary.getFuncDefines().Select(e => { return new GUIContent(e.editorName); }).ToArray();
        }
        public TriggerObjectDrawer(TriggerObjectDrawer parent, Transform transform)
        {
            this.parent = parent;
            this.transform = transform;
        }
    }
}