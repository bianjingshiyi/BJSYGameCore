using UnityEngine;
using BJSYGameCore;
using UnityEngine.SceneManagement;

namespace BJSYGameCore.UI
{
    public class UIObject : MonoBehaviour
    {
        [SerializeField]
        UIManager _ui;
        public UIManager ui
        {
            get
            {
                if (_ui == null)
                {
                    GameObject go = new GameObject("UIManager");
                    SceneManager.MoveGameObjectToScene(go, gameObject.scene);
                    _ui = go.AddComponent<UIManager>();
                }
                return _ui;
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
        public T to<T>() where T : UIObject
        {
            return this as T;
        }
    }
}