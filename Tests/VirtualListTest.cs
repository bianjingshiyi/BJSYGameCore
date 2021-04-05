using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BJSYGameCore.UI;
using UnityEngine.UI;

public struct TestDat
{
    public int data1;
    public string data2;
    public Vector2 data3;
}


public class VirtualListTest : MonoBehaviour
{
    VirtualList<UIObject> virtualList;
    GridLayoutGroup glg;
    VerticalLayoutGroup vlg;
    public UIObject itemObj;
    List<TestDat> testDatList = new List<TestDat>();

    int count = 0;

    void initData()
    {
        for(int i = 0; i < 1000; i++)
        {
            testDatList.Add(new TestDat { data1 = i, data2 = (1000-i).ToString(),data3 = new Vector2(i,-i) }) ;
        }
    }

    private void Awake()
    {
        initData();
    }

    private void OnEnable() {
        if(virtualList!= null) { virtualList.reShow(); }
    }

    private void Start()
    {
        glg = GetComponent<GridLayoutGroup>();
        if(glg)
            virtualList = new VirtualList<UIObject>(generateItem,glg);
        vlg = GetComponent<VerticalLayoutGroup>();
        if (vlg)
            virtualList = new VirtualList<UIObject>(generateItem, vlg);
        virtualList.onDisplayUIObj += setItemView;
        virtualList.TotalDataCount = testDatList.Count;
        initItemObj();
    }

    private void Update() {
        if (Input.GetKeyUp(KeyCode.R)) {
            virtualList.reShow();
        }
    }

    void initItemObj()
    {
        virtualList.reset();
        for(int i = 0; i < testDatList.Count; i++)
        {
            var item = virtualList.addItem();
            //if (item) item.transform.parent = transform;
        }
            
    }

    UIObject generateItem()
    {
        UIObject obj =  Instantiate(itemObj,transform);
        obj.gameObject.SetActive(true);
        obj.gameObject.name = $"obj{count++}";
        return obj;
    }

    void setItemView(int i,UIObject obj)
    {
        obj.getChild("data1").GetComponent<Text>().text = testDatList[i].data1.ToString();
        obj.getChild("data2").GetComponent<Text>().text = testDatList[i].data2.ToString();
        obj.getChild("data3").GetComponent<Text>().text = testDatList[i].data3.ToString();
    }

}
