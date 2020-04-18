using System.Linq;
using System.Collections.Generic;
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
        public RectTransform rectTransform
        {
            get { return transform as RectTransform; }
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
        #region Child
        public void addChild(RectTransform obj)
        {
            obj.SetParent(rectTransform);
        }
        public bool removeChild(RectTransform obj, bool destroy = false)
        {
            if (obj == null)
                return false;
            if (obj.parent == rectTransform)
            {
                obj.parent = null;
                if (destroy)
                {
                    Destroy(obj.gameObject);
                }
                return true;
            }
            else
                return false;
        }
        public RectTransform getChild(string name)
        {
            return rectTransform.Find(name) as RectTransform;
        }
        public RectTransform[] getChildren(bool includeNotActive = false)
        {
            List<RectTransform> childList = new List<RectTransform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if ((!includeNotActive || child.gameObject.activeSelf) && child is RectTransform rTrans)
                    childList.Add(rTrans);
            }
            return childList.ToArray();
        }
        #endregion
        #region Controller
        Animator _animator;
        public Animator animator
        {
            get
            {
                if (_animator == null)
                    _animator = GetComponent<Animator>();
                return _animator;
            }
        }
        [HideInInspector]
        [SerializeField]
        string[] _initStates;
        string[] initStates
        {
            get { return _initStates; }
        }
        public string getController(string name, string[] stateNames)
        {
            foreach (string stateName in stateNames)
            {
                if (animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex(name + "Controller")).IsName(stateName))
                    return stateName;
            }
            return null;
        }
        public void setController(string name, string value)
        {
            if (animator.runtimeAnimatorController == null)
                Debug.LogError("Animator没有Controller", gameObject);
            animator.Play(value, animator.GetLayerIndex(name + "Controller"));
        }
        protected virtual void Awake()
        {
            foreach (string initState in initStates)
            {
                string[] sArray = initState.Split('/');
                string controllerName = sArray[0];
                string stateName = sArray[1];
                setController(controllerName, stateName);
            }
        }
        #endregion
    }
}