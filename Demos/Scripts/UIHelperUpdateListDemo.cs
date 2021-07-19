using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BJSYGameCore.UI;
using System.Linq;

namespace BJSYGameCore.Demos
{
    public class UIHelperUpdateListDemo : MonoBehaviour
    {
        protected void Awake()
        {
            _countInputField.onEndEdit.AddListener(onEndEditCountInputField);
        }
        void onEndEditCountInputField(string text)
        {
            if (int.TryParse(text, out int count))
            {
                UIHelper.updateList(_listRoot, _listItemTemplate, _itemList, count, (i, item) =>
                    {
                        item.GetComponentInChildren<Text>().text = i.ToString();
                    }, item =>
                    {
                        item.GetComponent<UIHelperUpdateListDemoItem>().onClick += onClickListItem;
                    }, item =>
                    {
                        item.GetComponent<UIHelperUpdateListDemoItem>().onClick -= onClickListItem;
                    });
            }
        }
        void onClickListItem(RectTransform item)
        {
            Debug.Log(_itemList.IndexOf(item));
        }
        [SerializeField]
        RectTransform _listRoot;
        [SerializeField]
        RectTransform _listItemTemplate;
        [SerializeField]
        List<RectTransform> _itemList = new List<RectTransform>();
        [SerializeField]
        InputField _countInputField;
    }
}