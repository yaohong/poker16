using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HallControl : MonoBehaviour, IScene
{


    //public UIScrollView roomInfoPanelContainer;
    public GameObject createRoomDlgTempalte;
    public GameObject dlgParentObj;

    public RoomViewControl roomViewControl;
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
        Scheduling.Ins.ChangeScene(SceneType.ST_Login);
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
        GameObject createRoomDlgxObj = GameObject.Instantiate(createRoomDlgTempalte);
        CreateRoomDlgControl bc = createRoomDlgxObj.GetComponent<CreateRoomDlgControl>();
        bc.SetHallControl(this);

        createRoomDlgxObj.transform.parent = dlgParentObj.transform;
        createRoomDlgxObj.transform.localScale = Vector3.one;
        createRoomDlgxObj.transform.localPosition = Vector3.one;

    }

    public void CbCreateRoom(string roomName, int doubleDownScore, bool isAA, bool isLaiziPalyMethod, bool isOb, bool isRandom, bool isNotVoice, bool isSaftMode)
    {
        Debug.LogFormat("CreateRoomCallBack roomName[{0}]", roomName);
    }

    /// <summary>
    /// ///////////////////////////////////////////
    /// </summary>
    public void OnConnectSuccess()
    {

    }
    public void OnConnectFailed()
    {

    }
    public void OnDisconnect()
    {

    }

    public void OnCompletePacket(qp_server.qp_packet packet)
    {

    }

    public void EnterScene()
    {
        gameObject.SetActive(true);
    }
    public void ExitScene()
    {
        roomViewControl.ClearAllRoomInfo();
        gameObject.SetActive(false);
    }
}
