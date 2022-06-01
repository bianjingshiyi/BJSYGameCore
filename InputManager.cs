using UnityEngine.EventSystems;
using UnityEngine;
using System.Collections.Generic;
namespace BJSYGameCore
{
    public class InputManager : Manager
    {
        #region 公有方法
        public bool isPointerOver(RectTransform transform)
        {
            if (EventSystem.current is var eventSystem)
            {
                var input = eventSystem.currentInputModule.input;
                if (input.touchSupported)
                {
                    for (int i = 0; i < input.touchCount; i++)
                    {
                        var touch = input.GetTouch(i);
                        if (transform.rect.Contains(transform.InverseTransformPoint(touch.position)))
                            return true;
                    }
                }
                if (transform.rect.Contains(transform.InverseTransformPoint(input.mousePosition)))
                    return true;
                return false;
            }
            else
                return false;
        }
        public void OnUpdate()
        {
            if (EventSystem.current != null)
            {
                EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current)
                {
                    position = Input.mousePosition
                }, _raycastList);
            }
        }
        #endregion
        #region 私有方法
        #region 生命周期
        protected void Update()
        {
            OnUpdate();
        }
        #endregion
        #endregion
        #region 属性字段
        [SerializeField]
        private List<RaycastResult> _raycastList = new List<RaycastResult>();
        #endregion
    }
}