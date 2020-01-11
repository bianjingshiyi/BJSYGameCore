using System;
using System.Linq;
using UnityEngine;

namespace BJSYGameCore.UI
{
    public class UIList : UIObject
    {
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
            return GetComponentsInChildren(defaultItem.GetType()).Cast<UIObject>().Where(obj => obj != defaultItem).ToArray();
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
    }
}