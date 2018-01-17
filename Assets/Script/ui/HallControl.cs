using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HallControl : MonoBehaviour {


    //public UIScrollView roomInfoPanelContainer;
    public GameObject createRoomMsgBoxTempalte;
    public GameObject msgBoxParentObj;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}



    public void ExitClick()
    {
        Debug.LogError("ExitClick");
        if (!TcpManager.Ins.ConnectByIpPort("121.41.107.94", 18751))
        {
            Log.Logic("ConnectByIpPort failed");
        }
    }

    public void BuyClick()
    {
        Debug.LogError("BuyClick");
    }

    public void BindClick()
    {
        Debug.LogError("BindClick");
    }

    public void TaskClick()
    {
        Debug.LogError("TaskClick");
    }

    public void OptionClick()
    {
        Debug.LogError("OptionClick");
    }

    public void CreateRoomClick()
    {
        GameObject createRoomMsgBoxObj = GameObject.Instantiate(createRoomMsgBoxTempalte);
        CreateRoomMsgBoxControl bc = createRoomMsgBoxObj.GetComponent<CreateRoomMsgBoxControl>();
        bc.SetHallControl(this);

        createRoomMsgBoxObj.transform.parent = msgBoxParentObj.transform;
        createRoomMsgBoxObj.transform.localScale = Vector3.one;
        createRoomMsgBoxObj.transform.localPosition = Vector3.one;

    }

    public void CbCreateRoom(string roomName, int doubleDownScore, bool isAA, bool isLaiziPalyMethod, bool isOb, bool isRandom, bool isNotVoice, bool isSaftMode)
    {
        Debug.LogFormat("CreateRoomCallBack roomName[{0}]", roomName);
    }
}
