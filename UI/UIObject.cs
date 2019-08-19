using UnityEngine;
using BJSYGameCore;

namespace BJSYGameCore.UI
{
    public class UIObject : MonoBehaviour
    {
        [SerializeField]
        UIManager _manager;
        public UIManager manager
        {
            get
            {
                if (_manager == null)
                    _manager = this.findInstance<UIManager>();
                return _manager;
            }
        }
        public bool isDisplaying
        {
            get { return gameObject.activeSelf; }
        }
        public void display()
        {
            gameObject.SetActive(true);
            onDisplay();
        }
        protected virtual void onDisplay()
        {
        }
        public void hide()
        {
            gameObject.SetActive(false);
            onHide();
        }
        protected virtual void onHide()
        {
        }
    }
}