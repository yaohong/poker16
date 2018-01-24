using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetPacketHandle {

	// Use this for initialization
	public static void LoginRspHandle(qp_server.qp_login_rsp rsp)
    {
        //只处理正确的返回
        if (rsp.state != 0)
        {
            return;
        }

        GlobalData.Ins.userId = rsp.public_data.user_id;
        GlobalData.Ins.nickname = rsp.public_data.nick_name;
        GlobalData.Ins.avatarUrl = rsp.public_data.avatar_url;
        GlobalData.Ins.roomCardCount = rsp.private_data.room_card_count;

        if (rsp.room_data != null)
        {
            //初始化房间信息
            GlobalData.Ins.SetRoomData(rsp.room_data);
            Scheduling.Ins.ChangeScene(SceneType.ST_Room);
        }
        else
        {
            Scheduling.Ins.ChangeScene(SceneType.ST_Hall);
        }
    }

    public static void SendLoginReq()
    {
        qp_server.qp_login_req req = new qp_server.qp_login_req();
        req.account = GlobalData.Ins.loginAcc;
        req.pwd = GlobalData.Ins.loginPwd;

        byte[] buff = CmdBase.ProtoBufSerialize<qp_server.qp_login_req>(req);
        TcpManager.Ins.SendData((int)qp_server.ws_cmd.CMD_QP_LOGIN_REQ, buff);
    }

    public static void SendPing()
    {
        qp_server.qp_ping_req req = new qp_server.qp_ping_req();
        req.noop = 1;
        byte[] buff = CmdBase.ProtoBufSerialize<qp_server.qp_ping_req>(req);
        TcpManager.Ins.SendData((int)qp_server.ws_cmd.CMD_QP_PING_REQ, buff);
    }
}
