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

    private BlockedControl blockedControl;
    private CreateRoomDlgControl createRoomDlgControl;

    private float lastSendPingTime;
	void Start () {
        lastSendPingTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
        if (Scheduling.Ins.currentSceneType == SceneType.ST_Hall)
        {
            float curTime = Time.time;
            if (curTime - lastSendPingTime > GlobalData.Ins.PING_SPACE_TIME)
            {
                Log.Logic("send_ping cur=[{0}]", curTime);
                NetPacketHandle.SendPing();
                lastSendPingTime = curTime;
            }
        }
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

    public void JoinRoomClick()
    {

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
        NetPacketHandle.SendLoginReq();
        if (blockedControl)
        {
            blockedControl.SetState("链接成功正在验证账号密码");
        }
        Log.Logic("hall reconnect success");
    }
    public void OnConnectFailed()
    {
        //链接失败
        //直接切换到登陆场景
        Log.Logic("hall reconnect failed");
        Scheduling.Ins.ChangeScene(SceneType.ST_Login);
    }
    public void OnDisconnect()
    {
        //网络断开了重连
        if (!TcpManager.Ins.ConnectByIpPort(GlobalData.Ins.serverIp, GlobalData.Ins.serverPort))
        {
            Scheduling.Ins.ChangeScene(SceneType.ST_Login);
            return;
        }

        blockedControl = Common.Ins.CreateBlocked(dlgParentObj);
        blockedControl.SetState("服务器断开,正在重连");
    }

    public void OnCompletePacket(qp_server.qp_packet packet)
    {
        switch ((qp_server.ws_cmd)packet.cmd)
        {
            case qp_server.ws_cmd.CMD_QP_LOGIN_RSP:
                OnLoginRsp(CmdBase.ProtoBufDeserialize<qp_server.qp_login_rsp>(packet.serialized));
                break;
            case qp_server.ws_cmd.CMD_QP_CREATE_ROOM_RSP:
                OnCreateRoomRsp(CmdBase.ProtoBufDeserialize<qp_server.qp_create_room_rsp>(packet.serialized));
                break;
            case qp_server.ws_cmd.CMD_QP_JOIN_ROOM_RSP:
                OnJoinRoomRsp(CmdBase.ProtoBufDeserialize<qp_server.qp_join_room_rsp>(packet.serialized));
                break;
            case qp_server.ws_cmd.CMD_QP_PING_RSP:
                Log.Error("currentScene[HALL], ping_rsp");
                break;
            default:
                Log.Error("currentScene[HALL], unknown cmd={0}", packet.cmd);
                break;
        }
    }

    void OnLoginRsp(qp_server.qp_login_rsp rsp)
    {
        Log.Logic("hall login_rsp state={0}", rsp.state);
        DestoryBlocked();
        if (rsp.state != 0)
        {
            Scheduling.Ins.ChangeScene(SceneType.ST_Login);
        }
        else
        {
            NetPacketHandle.LoginRspHandle(rsp);
        }

    }

    void OnCreateRoomRsp(qp_server.qp_create_room_rsp rsp)
    {
        Log.Logic("OnCreateRoomRsp, state={0}",rsp.state);
        if (rsp.state == 0)
        {
            Log.Logic("create_room success, room_id={0}", rsp.room_id);
        }
        else
        {
            Log.Logic("create_room failed");
        }
    }

    void OnJoinRoomRsp(qp_server.qp_join_room_rsp rsp)
    {
        Log.Logic(
            "OnJoinRoomRsp, state={0}",
            rsp.result);
        if (rsp.result != 0)
        {
            Log.Logic("join_room failed");
        } 
        else
        {
            //设置房间信息
            GlobalData.Ins.SetRoomData(rsp.room_data);
            Scheduling.Ins.ChangeScene(SceneType.ST_Room);
        }
    }


    /// <summary>
    /// ///////////////////////////////
    /// </summary>

    public void EnterScene()
    {
        //做一些初始化操作
        lastSendPingTime = Time.time;
        Log.Logic("EnterScene[hall] lastSendPingTime={0}, space_time={1}", lastSendPingTime, GlobalData.Ins.PING_SPACE_TIME);
        gameObject.SetActive(true);
    }
    public void ExitScene()
    {
        roomViewControl.ClearAllRoomInfo();
        DestoryCreateRoomDlg();
        DestoryBlocked();
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

    void DestoryBlocked()
    {
        if (blockedControl != null)
        {
            GameObject.DestroyObject(blockedControl.gameObject);
            blockedControl = null;
        }
    }
}
