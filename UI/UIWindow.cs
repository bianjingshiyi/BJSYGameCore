using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

namespace BJSYGameCore.UI
{
    public class UIWindow : UIObject
    {
        UIWindow _parent;
        protected override void onHide()
        {
            base.onHide();
            if (_parent != null)
            {
                _parent.enableInteraction();
                _parent = null;
            }
        }
        public void display(UIWindow parent)
        {
            parent.disableInteraction();
            transform.parent = parent.transform;
            transform.SetAsLastSibling();
            _parent = parent;
            display();
        }
        Image _blocker = null;
        void disableInteraction()
        {
            _blocker = new GameObject("Blocker").AddComponent<Image>();
            _blocker.transform.parent = transform;
            _blocker.color = Color.clear;
            _blocker.rectTransform.anchorMin = Vector2.zero;
            _blocker.rectTransform.anchorMax = Vector2.one;
            _blocker.rectTransform.offsetMin = Vector2.zero;
            _blocker.rectTransform.offsetMax = Vector2.zero;
            _blocker.rectTransform.SetAsLastSibling();
        }
        void enableInteraction()
        {
            Destroy(_blocker.gameObject);
        }
    }
}
