using System.Linq;
using System.Collections.Generic;
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
        public int childCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < transform.childCount; i++)
                {
                    if (transform.GetChild(i).gameObject.activeSelf)
                        count++;
                }
                return count;
            }
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
    }
}