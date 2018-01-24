using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomControl : MonoBehaviourX, IScene
{
    public GameObject dlgParentObj;


    public UILabel playMethodLable;         //显示玩法的lable
    public UILabel modeLable;               //显示分配模式的lable
    public UILabel roomNumberLable;         //显示房间号的lable
    public RoomSeatUser[] seatUsers;        //四个位置的用户
	// Use this for initialization

    private BlockedControl blockedControl;
    private float lastSendPingTime;
	void Start () {
        lastSendPingTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
        if (Scheduling.Ins.currentSceneType == SceneType.ST_Room)
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


    /// <summary>
    /// IScene的接口
    /// </summary>
    public void OnConnectSuccess()
    {
        NetPacketHandle.SendLoginReq();
        if (blockedControl)
        {
            blockedControl.SetState("链接成功正在验证账号密码");
        }
        Log.Logic("room reconnect success");
    }
    public void OnConnectFailed()
    {
        //链接失败
        //直接切换到登陆场景
        Log.Logic("room reconnect failed");
        Scheduling.Ins.ChangeScene(SceneType.ST_Login);
    }
    public void OnDisconnect()
    {
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
            case qp_server.ws_cmd.CMD_QP_PING_RSP:
                Log.Error("currentScene[LOGIN], ping_rsp");
                break;
            default:
                Log.Error("currentScene[LOGIN], unknown cmd={0}", packet.cmd);
                break;
        }
    }

    void OnLoginRsp(qp_server.qp_login_rsp rsp)
    {
        Log.Logic("room login_rsp state={0}", rsp.state);
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

    public void EnterScene()
    {
        //初始化显示信息
        ShowPlayMethod(GlobalData.Ins.roomCfg.isAa, GlobalData.Ins.roomCfg.isLaiziPlaymethod, GlobalData.Ins.roomCfg.doubleDownScore);
        ShowMode(GlobalData.Ins.roomCfg.isRandom);
        ShowSeatUser();
        ShowRoomNumber(GlobalData.Ins.currentRoomId);
        lastSendPingTime = Time.time;
        Log.Logic("EnterScene[ROOM] lastSendPingTime={0}, space_time={1}", lastSendPingTime, GlobalData.Ins.PING_SPACE_TIME);
        for (int i = 0; i < seatUsers.Length; i++)
        {
            seatUsers[i].Standup();
        }
        gameObject.SetActive(true);
    }
    public void ExitScene()
    {
        GlobalData.Ins.ClearRoomData();
        for (int i=0; i<seatUsers.Length; i ++)
        {
            seatUsers[i].Standup();
        }
        //做清理操作
        gameObject.SetActive(false);

    }


    ////////////////////////////
    ///辅助函数
    ///
    void ShowPlayMethod(bool isAA, bool isLaizi, int doubleDownScore)
    {
        string aaStr = "";
        if (isAA)
        {
            aaStr = "玩家AA";
        }
        else 
        {
            aaStr = "房主全包";
        }

        string laiziStr = "";
        if (isLaizi)
        {
            laiziStr = "癞子玩法";
        }
        else 
        {
            laiziStr = "普通玩法";
        }
        string str = string.Format("{0}/{1}/双下{2}分", aaStr, laiziStr, doubleDownScore);
        playMethodLable.text = str;
    }

    void ShowMode(bool isRandom)
    {
        if (isRandom)
        {
            modeLable.text = "随机队友";
        }
        else
        {
            modeLable.text = "固定队友";
        }
    }

    void ShowRoomNumber(int roomNumber)
    {
        roomNumberLable.text = string.Format("{0}", roomNumber);
    }

    void ShowSeatUser()
    {
        foreach (var item in GlobalData.Ins.allRoomUsers)
        {
            RoomUser roomUser = item.Value;
            if (roomUser.seatNumber != -1)
            {
                RoomSeatUser seatUser = seatUsers[roomUser.seatNumber];
                seatUser.Sitdown(roomUser.nickname, roomUser.avatarUrl);
            }
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
