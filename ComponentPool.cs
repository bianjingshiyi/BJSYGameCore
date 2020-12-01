using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
namespace BJSYGameCore
{
    [Serializable]
    public class ComponentPool<T> where T : Component
    {
        #region 公共成员
        public ComponentPool(Transform root, T origin)
        {
            _root = root;
            _origin = origin;
        }
        public void add(T component)
        {
            if (component == null)
                throw new ArgumentNullException("component");
            cleanNullAndRepeat(component);
            _itemList.Add(component);
            component.transform.parent = _root;
        }
        public void insert(T component, int index)
        {
            if (component == null)
                throw new ArgumentNullException("component");
            cleanNullAndRepeat(component);
            _itemList.Insert(index, component);
            component.transform.parent = _root;
        }
        public virtual T create()
        {
            T component;
            while (_poolList.Count > 0)
            {
                component = _poolList[0];
                _poolList.RemoveAt(0);
                if (component == null)
                    continue;
                _itemList.Add(component);
                if (onCreate != null)
                    onCreate(component);
                component.gameObject.SetActive(true);
                return component;
            }
            component = Object.Instantiate(_origin, _root);
            if (onCreate != null)
                onCreate(component);
            component.gameObject.SetActive(true);
            _itemList.Add(component);
            return component;
        }
        public virtual bool remove(T component)
        {
            for (int i = 0; i < _itemList.Count; i++)
            {
                if (_itemList[i] == null)
                {
                    _itemList.RemoveAt(i);
                    i--;
                    continue;
                }
                if (_itemList[i] == component)
                {
                    removeAt(i);
                    return true;
                }
            }
            return false;
        }
        public virtual void removeAt(int index)
        {
            T component = _itemList[index];
            _itemList.RemoveAt(index);
            if (component == null)
                return;
            if (onRemove != null)
                onRemove(component);
            _poolList.Add(component);
            component.gameObject.SetActive(false);
        }
        public T this[int index]
        {
            get
            {
                cleanNull();
                return _itemList[index];
            }
        }
        public int indexOf(T component)
        {
            return _itemList.IndexOf(component);
        }
        public int count
        {
            get
            {
                cleanNull();
                return _itemList.Count;
            }
        }
        /// <summary>
        /// 创建或删除Component直到数量等于指定的值。
        /// </summary>
        /// <param name="count"></param>
        public void setCount(int count)
        {
            while (_itemList.Count < count)
            {
                create();
            }
            while (_itemList.Count > count)
            {
                removeAt(_itemList.Count - 1);
            }
        }
        public event Action<T> onCreate;
        public event Action<T> onRemove;
        #endregion
        #region 私有成员
        private void cleanNull()
        {
            for (int i = 0; i < _itemList.Count; i++)
            {
                if (_itemList[i] == null)
                {
                    _itemList.RemoveAt(i);
                    i--;
                    continue;
                }
            }
        }
        private void cleanNullAndRepeat(T component)
        {
            for (int i = 0; i < _itemList.Count; i++)
            {
                if (_itemList[i] == null)
                {
                    _itemList.RemoveAt(i);
                    i--;
                    continue;
                }
                if (_itemList[i] == component)
                    throw new InvalidOperationException(this + "中已经存在" + component);
            }
        }
        [SerializeField]
        Transform _root;
        [SerializeField]
        T _origin;
        [SerializeField]
        List<T> _itemList = new List<T>();
        [SerializeField]
        List<T> _poolList = new List<T>();
        #endregion
    }
}
