using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UIType
{
    UIT_Login,                  //登陆场景
    UIT_Hall,                   //大厅
    UIT_Game                    //游戏界面
}
public class Scheduling : MonoBehaviourX
{

    static Scheduling ins = null;

    private UIType curUi = UIType.UIT_Login;

    public static Scheduling Ins
    {
        get
        {
            return ins;
        }
    }

    private void Awake()
    {
        if (ins != null)
        {
            Debug.LogError("TcpManager Recreate!!");
        }

        ins = this;
    }
	// Use this for initialization
	void Start () {
        scope.Listen("ConnectSuccess", new CALLBACK(OnConnectSuccess));
        scope.Listen("ConnectFailed", new CALLBACK(OnConnectFailed));
        scope.Listen("Disconnect", new CALLBACK(OnDisconnect));
        scope.Listen("CompletePacket", new CALLBACK(OnCompletePacket));
	}

    void OnConnectSuccess(object []args)
    {
        bool isRereconnect = (bool)args[0];
        Log.Logic("connect success, isRereconnect={0}", isRereconnect);

        //发送登陆协议
        qp_server.qp_login_req req = new qp_server.qp_login_req();
        req.account = "yaohong";
        req.pwd = "231344781";

        byte[] buff = CmdBase.ProtoBufSerialize<qp_server.qp_login_req>(req);
        Log.LogByte(buff, buff.Length);
        TcpManager.Ins.SendData((int)qp_server.ws_cmd.CMD_QP_LOGIN_REQ, buff);

    }

    void OnConnectFailed(object[] args)
    {
        bool isRereconnect = (bool)args[0];
        Log.Logic("connect failed, isRereconnect={0}", isRereconnect);
    }

    void OnDisconnect(object[] args)
    {
        Log.Logic("disconnect");
    }

    void OnCompletePacket(object[] args)
    {
        IncomingPacket packet = args[0] as IncomingPacket;
        if (packet == null)
        {
            return;
        }

        qp_server.qp_packet qpPacket =  CmdBase.ProtoBufDeserialize<qp_server.qp_packet>(packet.recvBuff);
        switch ((qp_server.ws_cmd)qpPacket.cmd)
        {
            case qp_server.ws_cmd.CMD_QP_LOGIN_RSP:
                OnLoginRsp(CmdBase.ProtoBufDeserialize<qp_server.qp_login_rsp>(qpPacket.serialized));
                break;
            case qp_server.ws_cmd.CMD_QP_CREATE_ROOM_RSP:
                OnCreateRoomRsp(CmdBase.ProtoBufDeserialize<qp_server.qp_create_room_rsp>(qpPacket.serialized));
                break;
            case qp_server.ws_cmd.CMD_QP_JOIN_ROOM_RSP:
                break;
            case qp_server.ws_cmd.CMD_QP_JOIN_ROOM_PUSH:
                break;
            case qp_server.ws_cmd.CMD_QP_SITDOWN_RSP:
                break;
            case qp_server.ws_cmd.CMD_QP_SITDOWN_PUSH:
                break;
            case qp_server.ws_cmd.CMD_QP_STANDUP_RSP:
                break;
            case qp_server.ws_cmd.CMD_QP_STANDUP_PUSH:
                break;
            case qp_server.ws_cmd.CMD_QP_EXIT_ROOM_RSP:
                break;
            case qp_server.ws_cmd.CMD_QP_EXIT_ROOM_PUSH:
                break;
            case qp_server.ws_cmd.CMD_QP_GAME_DATA:
                break;
            case qp_server.ws_cmd.CMD_QP_PING_RSP:
                break;
            default:
                Log.Error("unknown cmd={0}", qpPacket.cmd);
                break;
        }

    }

    void OnLoginRsp(qp_server.qp_login_rsp rsp)
    {
        Log.Logic(
            "state={0}, nickname={1} avatarUrl={2} roomCardCount={3}",
            rsp.state,
            rsp.public_data.nick_name,
            rsp.public_data.avatar_url,
            rsp.private_data.room_card_count);
    }

    void OnCreateRoomRsp(qp_server.qp_create_room_rsp rsp)
    {

    }
	

    void OnJoinRoomRsp(qp_server.qp_join_room_rsp rsp)
    {

    }

    void OnJoinRoomPush(qp_server.qp_join_room_push push)
    {

    }

    void OnSitdownRsp(qp_server.qp_sitdown_rsp rsp)
    {

    }

    void OnSitdownPush(qp_server.qp_sitdown_push push)
    {

    }

    void OnStandupRsp(qp_server.qp_standup_rsp rsp)
    {

    }

    void OnStandupPush(qp_server.qp_standup_push push)
    {

    }


	// Update is called once per frame
}
