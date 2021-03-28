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
    public UIObject itemObj;
    List<TestDat> testDatList = new List<TestDat>();
 
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

    private void Start()
    {
        glg = GetComponent<GridLayoutGroup>();
        virtualList = new VirtualList<UIObject>(generateItem,glg);
        virtualList.onDisPlayItem += setItemView;
        initItemObj();
    }

    void initItemObj()
    {
        for(int i=0;i<testDatList.Count;i++)
            virtualList.addItem();
    }

    UIObject generateItem()
    {
        UIObject obj =  Instantiate(itemObj,transform);
        obj.gameObject.SetActive(true);
        return obj;
    }

    void setItemView(int i,UIObject obj)
    {
        obj.getChild("data1").GetComponent<Text>().text = testDatList[i].data1.ToString();
        obj.getChild("data2").GetComponent<Text>().text = testDatList[i].data2.ToString();
        obj.getChild("data3").GetComponent<Text>().text = testDatList[i].data3.ToString();
    }

}
