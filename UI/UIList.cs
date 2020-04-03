using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;

namespace BJSYGameCore.UI
{
    public class UIList : UIObject
    {
        #region Item
        [SerializeField]
        UIObject _defaultItem;
        public UIObject defaultItem
        {
            get { return _defaultItem; }
            set { _defaultItem = value; }
        }
        public UIObject addItem()
        {
            UIObject item = Instantiate(defaultItem, transform);
            item.gameObject.SetActive(true);
            return item;
        }
        public UIObject[] getItems()
        {
            if (defaultItem == null)
                return new UIObject[0];
            List<UIObject> itemList = new List<UIObject>();
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (!child.gameObject.activeSelf)
                    continue;
                if (child.GetComponent(defaultItem.GetType()) is UIObject item && item != defaultItem)
                {
                    itemList.Add(item);
                }
                else
                    continue;
            }
            return itemList.ToArray();
        }
        public int itemCount
        {
            get { return getItems().Length; }
        }
        public bool removeItem(UIObject item)
        {
            if (item == null)
                return false;
            if (item.transform.parent == transform)
            {
                Destroy(item.gameObject);
                return true;
            }
            else
                return false;
        }
        public void clearItems()
        {
            foreach (UIObject item in getItems())
            {
                removeItem(item);
            }
        }
        #endregion
    }
    public abstract class UIList<T> : UIList, IEnumerable<T> where T : UIObject
    {
        public new T defaultItem
        {
            get
            {
                if (base.defaultItem == null)
                    defaultItem = getDefaultItem();
                return base.defaultItem as T;
            }
            set
            {
                base.defaultItem = value as T;
            }
        }
        protected abstract T getDefaultItem();
        public new virtual T addItem()
        {
            return base.addItem() as T;
        }
        public new T[] getItems()
        {
            return base.getItems().Cast<T>().ToArray();
        }
        public bool removeItem(T item)
        {
            return base.removeItem(item);
        }
        public void updateItems<TObject>(IEnumerable<TObject> objects, Func<T, TObject, bool> isMatchItem, Action<T, TObject> updateItem)
        {
            List<T> remainedItemList = new List<T>(getItems());
            foreach (TObject obj in objects)
            {
                T item = remainedItemList.FirstOrDefault(i => isMatchItem(i, obj));
                if (item == null)
                {
                    item = addItem();
                    updateItem(item, obj);
                }
                else
                {
                    updateItem(item, obj);
                    remainedItemList.Remove(item);
                }
            }
            foreach (T item in remainedItemList)
            {
                removeItem(item);
            }
        }
        public void sortItems(Comparison<T> comparison)
        {
            List<T> items = new List<T>(getItems());
            items.Sort(comparison);
            while (items.Count > 0)
            {
                T item = items[items.Count - 1];
                item.transform.SetSiblingIndex(0);
                items.RemoveAt(items.Count - 1);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return getItems().Cast<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return getItems().GetEnumerator();
        }
    }
}