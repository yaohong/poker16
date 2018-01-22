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

    private BlockedControl currentBlockedControl;
    private CreateRoomDlgControl createRoomDlgControl;
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
        //弹出创建房间的对话框
        CreateCreateRoomDlg();
    }

    /// <summary>
    /// 子窗口(创建房间窗口)的回调
    /// </summary>
    /// <param name="roomName"></param>
    /// <param name="doubleDownScore"></param>
    /// <param name="isAA"></param>
    /// <param name="isLaiziPalyMethod"></param>
    /// <param name="isOb"></param>
    /// <param name="isRandom"></param>
    /// <param name="isNotVoice"></param>
    /// <param name="isSaftMode"></param>
    public void CreateRoomDlg_CreateBtnClick(string roomName, int doubleDownScore, bool isAA, bool isLaiziPalyMethod, bool isOb, bool isRandom, bool isNotVoice, bool isSafeMode)
    {
        Log.Logic("CreateRoomCallBack roomName[{0}], doubleDownScore[{1}], isAA[{2}], islaizi[{3}], isOb[{4}], isRandom[{5}], isNotVoice[{6}], isSafeMode[{7}]",
            roomName, doubleDownScore, isAA, isLaiziPalyMethod, isOb, isRandom, isNotVoice, isSafeMode);

        qp_server.qp_create_room_req req = new qp_server.qp_create_room_req();
        qp_server.pb_room_cfg cfg = new qp_server.pb_room_cfg();
        cfg.room_name = roomName;
        cfg.is_aa = isAA;
        cfg.double_down_score = doubleDownScore;
        cfg.is_laizi_playmethod = isLaiziPalyMethod;
        cfg.is_ob = isOb;
        cfg.is_random = isRandom;
        cfg.is_not_voice = isNotVoice;
        cfg.is_safe_mode = isSafeMode;
        cfg.lock_userid_list.Add(10001);
        cfg.lock_userid_list.Add(10002);
        req.cfg = cfg;

        byte[] buff = CmdBase.ProtoBufSerialize<qp_server.qp_create_room_req>(req);
        TcpManager.Ins.SendData((int)qp_server.ws_cmd.CMD_QP_CREATE_ROOM_REQ, buff);
    }

    public void CreateRoomDlg_ExitClick()
    {
        DestoryCreateRoomDlg();
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
        //网络断开了重连
    }

    public void OnCompletePacket(qp_server.qp_packet packet)
    {
        switch ((qp_server.ws_cmd)packet.cmd)
        {
            //case qp_server.ws_cmd.CMD_QP_LOGIN_RSP:
            //    OnLoginRsp(CmdBase.ProtoBufDeserialize<qp_server.qp_login_rsp>(packet.serialized));
            //    break;
            case qp_server.ws_cmd.CMD_QP_CREATE_ROOM_RSP:
                OnCreateRoomRsp(CmdBase.ProtoBufDeserialize<qp_server.qp_create_room_rsp>(packet.serialized));
                break;
            case qp_server.ws_cmd.CMD_QP_JOIN_ROOM_RSP:
                OnJoinRoomRsp(CmdBase.ProtoBufDeserialize<qp_server.qp_join_room_rsp>(packet.serialized));
                break;
            //case qp_server.ws_cmd.CMD_QP_JOIN_ROOM_PUSH:
            //    OnJoinRoomPush(CmdBase.ProtoBufDeserialize<qp_server.qp_join_room_push>(qpPacket.serialized));
            //    break;
            //case qp_server.ws_cmd.CMD_QP_SITDOWN_RSP:
            //    OnSitdownRsp(CmdBase.ProtoBufDeserialize<qp_server.qp_sitdown_rsp>(qpPacket.serialized));
            //    break;
            //case qp_server.ws_cmd.CMD_QP_SITDOWN_PUSH:
            //    OnSitdownPush(CmdBase.ProtoBufDeserialize<qp_server.qp_sitdown_push>(qpPacket.serialized));
            //    break;
            //case qp_server.ws_cmd.CMD_QP_STANDUP_RSP:
            //    OnStandupRsp(CmdBase.ProtoBufDeserialize<qp_server.qp_standup_rsp>(qpPacket.serialized));
            //    break;
            //case qp_server.ws_cmd.CMD_QP_STANDUP_PUSH:
            //    OnStandupPush(CmdBase.ProtoBufDeserialize<qp_server.qp_standup_push>(qpPacket.serialized));
            //    break;
            //case qp_server.ws_cmd.CMD_QP_EXIT_ROOM_RSP:
            //    OnExitRoomRsp(CmdBase.ProtoBufDeserialize<qp_server.qp_exit_room_rsp>(qpPacket.serialized));
            //    break;
            //case qp_server.ws_cmd.CMD_QP_EXIT_ROOM_PUSH:
            //    OnExitRoomPush(CmdBase.ProtoBufDeserialize<qp_server.qp_exit_room_push>(qpPacket.serialized));
            //    break;
            //case qp_server.ws_cmd.CMD_QP_GAME_DATA:
            //    OnGameData(CmdBase.ProtoBufDeserialize<qp_server.qp_game_data>(qpPacket.serialized));
            //    break;
            //case qp_server.ws_cmd.CMD_QP_PING_RSP:
            //    break;
            default:
                Log.Error("currentScene[LOGIN], unknown cmd={0}", packet.cmd);
                break;
        }
    }

    void OnCreateRoomRsp(qp_server.qp_create_room_rsp rsp)
    {
        Log.Logic(
            "OnCreateRoomRsp, state={0}, room_id={1}",
            rsp.state,
            rsp.room_idSpecified ? rsp.room_id : 0);

    }

    void OnJoinRoomRsp(qp_server.qp_join_room_rsp rsp)
    {
        Log.Logic(
            "OnJoinRoomRsp, state={0}",
            rsp.result);

    }


    /// <summary>
    /// ///////////////////////////////
    /// </summary>

    public void EnterScene()
    {
        //做一些初始化操作
        gameObject.SetActive(true);
    }
    public void ExitScene()
    {
        roomViewControl.ClearAllRoomInfo();
        DestoryCreateRoomDlg();
        gameObject.SetActive(false);
    }


    /////////////////////////////////////////

    void CreateCreateRoomDlg()
    {
        DestoryCreateRoomDlg();

        GameObject createRoomDlgObj = GameObject.Instantiate(createRoomDlgTempalte);
        CreateRoomDlgControl control = createRoomDlgObj.GetComponent<CreateRoomDlgControl>();
        control.SetHallControl(this);

        createRoomDlgObj.transform.parent = dlgParentObj.transform;
        createRoomDlgObj.transform.localScale = Vector3.one;
        createRoomDlgObj.transform.localPosition = Vector3.one;

        createRoomDlgControl = control;
    }

    void DestoryCreateRoomDlg()
    {
        if (createRoomDlgControl != null)
        {
            GameObject.DestroyObject(createRoomDlgControl.gameObject);
            createRoomDlgControl = null;
        }
    }
}
