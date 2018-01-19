using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomViewControl : MonoBehaviour {

	// Use this for initialization
    public GameObject roomInfoPanelInitLocationObj;
    public GameObject roomInfoPanelTempalte;
    public GameObject roomScrollViewObj;

    //存放当前所有的roomPanel
    private List<GameObject> allRoomPanelObjects = new List<GameObject>();

    private Vector3 initVec;
    private float yOffset;
	void Start () {
		//计算初始化的位置
        initVec = roomInfoPanelInitLocationObj.transform.localPosition;
        //计算新增面板的偏移量
        UIWidget initWidget = roomInfoPanelInitLocationObj.GetComponent<UIWidget>();
        yOffset = initWidget.localSize.y + 10;
        Debug.LogErrorFormat("yOffset={0}", yOffset);
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    public void RefreshRoomClick()
    {
        int roomPanelCount = allRoomPanelObjects.Count;
        GameObject roomInfoPanel = GameObject.Instantiate(roomInfoPanelTempalte);
        roomInfoPanel.transform.parent = roomScrollViewObj.transform;
        roomInfoPanel.transform.localScale = Vector3.one;
        roomInfoPanel.transform.localPosition = new Vector3(initVec.x, initVec.y - roomPanelCount * yOffset, initVec.z);
        RoomInfoControl infoControl = roomInfoPanel.GetComponent<RoomInfoControl>();
        infoControl.SetViewControl(this);
        infoControl.RoomId = roomPanelCount;
        infoControl.RoomState = ERoomState.RS_Ready;
        allRoomPanelObjects.Add(roomInfoPanel);
    }

    public void ClearAllRoomInfo()
    {
        foreach(var roomInfo in allRoomPanelObjects)
        {
            GameObject.DestroyObject(roomInfo);
        }
        allRoomPanelObjects.Clear();
    }


    public void RoomInfoClick(RoomInfoControl infoControl)
    {
        Debug.LogErrorFormat("roomId[{0}] Click", infoControl.RoomId);
    }
}
