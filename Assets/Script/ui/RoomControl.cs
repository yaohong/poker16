using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomControl : MonoBehaviourX, IScene
{


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    /// <summary>
    /// IScene的接口
    /// </summary>
    void OnConnectSuccess()
    {

    }
    void OnConnectFailed()
    {

    }
    void OnDisconnect()
    {

    }

    void OnCompletePacket(qp_server.qp_packet packet)
    {

    }

    void EnterScene()
    {
        gameObject.SetActive(true);
    }
    void ExitScene()
    {
        GlobalData.Ins.allRoomUsers.Clear();
        GlobalData.Ins.currentRoomId = -1;
        GlobalData.Ins.currentSeatNumber = -1;
        //做清理操作
        gameObject.SetActive(false);

    }
}
