using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomControl : MonoBehaviourX, IScene
{
    public GameObject dlgParentObj;

    public GameObject sitdownOrStandupObj;
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

    public void ExitBtnClick() 
    {
        qp_server.qp_exit_room_req req = new qp_server.qp_exit_room_req();
        req.seat_num = GlobalData.Ins.currentSeatNumber;
        byte[] bin = CmdBase.ProtoBufSerialize<qp_server.qp_exit_room_req>(req);
        TcpManager.Ins.SendData((int)qp_server.ws_cmd.CMD_QP_EXIT_ROOM_REQ, bin);
    }


    public void StandupOrSitdownBtnClick()
    {
        if (GlobalData.Ins.currentSeatNumber == -1)
        {
            //坐下
            qp_server.qp_sitdown_req req = new qp_server.qp_sitdown_req();
            req.seat_num = -1;
            byte[] bin = CmdBase.ProtoBufSerialize<qp_server.qp_sitdown_req>(req);
            TcpManager.Ins.SendData((int)qp_server.ws_cmd.CMD_QP_SITDOWN_REQ, bin);
        }
        else
        {
            //起立
            qp_server.qp_standup_req req = new qp_server.qp_standup_req();
            req.seat_num = GlobalData.Ins.currentSeatNumber;
            byte[] bin = CmdBase.ProtoBufSerialize<qp_server.qp_standup_req>(req);
            TcpManager.Ins.SendData((int)qp_server.ws_cmd.CMD_QP_STANDUP_REQ, bin);
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
        DestoryBlocked();
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
            case qp_server.ws_cmd.CMD_QP_JOIN_ROOM_PUSH:
                OnJoinRoomPush(CmdBase.ProtoBufDeserialize<qp_server.qp_join_room_push>(packet.serialized));
                break;
            case qp_server.ws_cmd.CMD_QP_SITDOWN_RSP:
                OnSitdownRsp(CmdBase.ProtoBufDeserialize<qp_server.qp_sitdown_rsp>(packet.serialized));
                break;
            case qp_server.ws_cmd.CMD_QP_SITDOWN_PUSH:
                OnSitdownPush(CmdBase.ProtoBufDeserialize<qp_server.qp_sitdown_push>(packet.serialized));
                break;
            case qp_server.ws_cmd.CMD_QP_STANDUP_RSP:
                OnStandupRsp(CmdBase.ProtoBufDeserialize<qp_server.qp_standup_rsp>(packet.serialized));
                break;
            case qp_server.ws_cmd.CMD_QP_STANDUP_PUSH:
                OnStandupPush(CmdBase.ProtoBufDeserialize<qp_server.qp_standup_push>(packet.serialized));
                break;
            case qp_server.ws_cmd.CMD_QP_EXIT_ROOM_RSP:
                OnExitRoomRsp(CmdBase.ProtoBufDeserialize<qp_server.qp_exit_room_rsp>(packet.serialized));
                break;
            case qp_server.ws_cmd.CMD_QP_EXIT_ROOM_PUSH:
                OnExitRoomPush(CmdBase.ProtoBufDeserialize<qp_server.qp_exit_room_push>(packet.serialized));
                break;
            case qp_server.ws_cmd.CMD_QP_ROOM_DISSMISS:
                OnRoomDissmiss(CmdBase.ProtoBufDeserialize<qp_server.qp_room_dissmiss>(packet.serialized));
                break;
            case qp_server.ws_cmd.CMD_QP_ROOM_KICK:
                OnRoomKick(CmdBase.ProtoBufDeserialize<qp_server.qp_room_kick>(packet.serialized));
                break;
            case qp_server.ws_cmd.CMD_QP_KICK:
                OnKick(CmdBase.ProtoBufDeserialize<qp_server.qp_kick>(packet.serialized));
                break;
            case qp_server.ws_cmd.CMD_QP_PING_RSP:
                Log.Error("currentScene[ROOM], ping_rsp");
                break;
            default:
                Log.Error("currentScene[ROOM], unknown cmd={0}", packet.cmd);
                break;
        }
    }

    void OnLoginRsp(qp_server.qp_login_rsp rsp)
    {
        Log.Logic("room login_rsp state={0}", rsp.state);
        if (rsp.state != 0)
        {
            Scheduling.Ins.ChangeScene(SceneType.ST_Login);
        }
        else
        {
            NetPacketHandle.LoginRspHandle(rsp);
        }

    }

    void OnJoinRoomPush(qp_server.qp_join_room_push push)
    {
        
        qp_server.pb_user_public_data publicData = push.public_data;
        Log.Logic("room JoinRoomPush {0}", publicData.user_id);
        //将玩家数据添加到缓存
        if (GlobalData.Ins.allRoomUsers.ContainsKey(publicData.user_id))
        {
            Log.Warning("user_id[{0}] exist.", publicData.user_id);
            return;
        }

        RoomUser newRoomUser = new RoomUser(publicData.user_id, publicData.nick_name, publicData.avatar_url, -1);
        GlobalData.Ins.allRoomUsers[publicData.user_id] = newRoomUser;
    }


    void OnSitdownRsp(qp_server.qp_sitdown_rsp rsp)
    {
        Log.Logic("room SitdownRsp, state={0} ", rsp.result);
        if (rsp.result == 0)
        {
            //成功坐下
            Log.Logic("serverNumber={0} ", rsp.seat_num);
            RoomUser roomUser = GlobalData.Ins.allRoomUsers[GlobalData.Ins.userId];
            roomUser.seatNumber = rsp.seat_num;
            seatUsers[roomUser.seatNumber].Sitdown(roomUser.nickname, roomUser.avatarUrl);

            GlobalData.Ins.currentSeatNumber = rsp.seat_num;
            RefreshBtnPic();
        }
    }

    void OnSitdownPush(qp_server.qp_sitdown_push push)
    {
        Log.Logic("room SitdownPush {0} {1}", push.user_id, push.seat_num);
        RoomUser roomUser = GlobalData.Ins.allRoomUsers[push.user_id];
        roomUser.seatNumber = push.seat_num;
        seatUsers[push.seat_num].Sitdown(roomUser.nickname, roomUser.avatarUrl);

    }

    void OnStandupRsp(qp_server.qp_standup_rsp rsp)
    {
        Log.Logic("room StandupRsp {0}", rsp.state);
        if (rsp.state == 0)
        {
            RoomUser roomUser = GlobalData.Ins.allRoomUsers[GlobalData.Ins.userId];
            seatUsers[roomUser.seatNumber].Standup();
            roomUser.seatNumber = -1;
            GlobalData.Ins.currentSeatNumber = -1;

            RefreshBtnPic();
        }
    }

    void OnStandupPush(qp_server.qp_standup_push push)
    {
        Log.Logic("room StandupPush {0}", push.seat_num);
        int standupUserId = 0;
        foreach(var item in GlobalData.Ins.allRoomUsers)
        {
            if (item.Value.seatNumber == push.seat_num)
            {
                standupUserId = item.Key;
                break;
            }
        }

        if (standupUserId == 0)
        {
            Log.Error("standupUserId = 0");
            return;
        }
        RoomUser roomUser = GlobalData.Ins.allRoomUsers[standupUserId];
        seatUsers[push.seat_num].Standup();
        roomUser.seatNumber = -1;
    }

    void OnExitRoomRsp(qp_server.qp_exit_room_rsp rsp)
    {
        Log.Logic("room ExitRoomRsp {0}", rsp.result);
        if (rsp.result == 0)
        {
            //退出成功
            Scheduling.Ins.ChangeScene(SceneType.ST_Hall);
        }
    }

    void OnExitRoomPush(qp_server.qp_exit_room_push push)
    {
        Log.Logic("room ExitRoomPush {0} {1}", push.user_id, push.seat_num);
        if (!GlobalData.Ins.allRoomUsers.ContainsKey(push.user_id))
        {
            Log.Error("user_id[{0}] not exist.", push.user_id);
            return;
        }

        RoomUser roomUser = GlobalData.Ins.allRoomUsers[push.user_id];
        if (roomUser.seatNumber != push.seat_num)
        {
            Log.Error("user_id[{0}] server_seatnumber={1} local_seatnumber={2}.", push.user_id, push.seat_num, roomUser.seatNumber);
            return;
        }

        GlobalData.Ins.allRoomUsers.Remove(push.user_id);
        if (roomUser.seatNumber != -1)
        {
            //在座位上
            seatUsers[roomUser.seatNumber].Standup();
        }
    }


    void OnRoomDissmiss(qp_server.qp_room_dissmiss msg)
    {
        Log.Logic("room RoomDissmiss {0} {1}", msg.room_id, msg.type);
        if (msg.room_id != GlobalData.Ins.currentRoomId)
        {
            //和客户端存的房间ID不一样
            Log.Error("RoomDissmiss, serverRoomId={0}, localServerRoomId={1}", msg.room_id, GlobalData.Ins.currentRoomId);
            return;
        }

        Scheduling.Ins.ChangeScene(SceneType.ST_Hall);
    }

    void OnRoomKick(qp_server.qp_room_kick notify)
    {
        Log.Logic("room RoomKick {0} {1} {2}", notify.room_id, notify.type, notify.user_id);
        if (notify.room_id != GlobalData.Ins.currentRoomId || notify.user_id != GlobalData.Ins.userId)
        {
            //和客户端存的房间ID不一样
            Log.Error("RoomDissmiss, serverRoomId={0}, localServerRoomId={1}, serverUserId={2}, selfId={3}",
                notify.room_id, GlobalData.Ins.currentRoomId, notify.user_id, GlobalData.Ins.userId);
            return;
        }
        Scheduling.Ins.ChangeScene(SceneType.ST_Hall);
    }

    void OnKick(qp_server.qp_kick kick)
    {
        //被T了弹到主界面
        Scheduling.Ins.ChangeScene(SceneType.ST_Login);
    }

    /// <summary>
    /// IScene接口的实现
    /// </summary>
    public void EnterScene()
    {
        //初始化显示信息
        
        for (int i = 0; i < seatUsers.Length; i++)
        {
            seatUsers[i].Standup();
        }
        RefreshBtnPic();
        ShowPlayMethod(GlobalData.Ins.roomCfg.isAa, GlobalData.Ins.roomCfg.isLaiziPlaymethod, GlobalData.Ins.roomCfg.doubleDownScore);
        ShowMode(GlobalData.Ins.roomCfg.isRandom);
        ShowSeatUser();
        ShowRoomNumber(GlobalData.Ins.currentRoomId);
        lastSendPingTime = Time.time;
        Log.Logic("EnterScene[ROOM] lastSendPingTime={0}, space_time={1}", lastSendPingTime, GlobalData.Ins.PING_SPACE_TIME);
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
        DestoryBlocked();
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

    void RefreshBtnPic()
    {
        UIButton btn = sitdownOrStandupObj.GetComponent<UIButton>();
        int seatNumber = GlobalData.Ins.currentSeatNumber;
        if (seatNumber == -1)
        {
            btn.normalSprite = "坐下@2x";
        }
        else
        {
            btn.normalSprite = "站起@2x";
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
