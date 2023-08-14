using System.Collections;
using UnityEngine;

namespace BJSYGameCore.UI
{
    public abstract class UIPanel : UIObject
    {
        public virtual void Initialize(UIManager uiManager)
        {
            _uiManager = uiManager;
        }
        public virtual void Show()
        {
            transform.SetAsLastSibling();
            gameObject.SetActive(true);
            OnShow();
        }
        public virtual void Hide()
        {
            OnHide();
            gameObject.SetActive(false);
        }
        public virtual IEnumerator PlayShowAnimation()
        {
            yield return null;
        }
        public virtual IEnumerator PlayHideAnimation()
        {
            yield return null;
        }
        protected virtual void OnShow()
        {
        }
        protected virtual void OnHide()
        {
        }

        public UIManager uiManager => _uiManager;
        [SerializeField]
        protected UIManager _uiManager;
    }
}