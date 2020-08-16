using UnityEngine.EventSystems;
using UnityEngine;
using System.Collections.Generic;
namespace BJSYGameCore.UI
{
    public class InputManager : Manager
    {
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
    }
}