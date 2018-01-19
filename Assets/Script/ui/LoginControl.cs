using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LoginControl : MonoBehaviourX, IScene
{

    //checkbox
    public UIToggle agreedCheckBox;
	// Use this for initialization
    public GameObject accountLoginDlgTempalte;
    public GameObject dlgParentObj;

    /// <summary>
    /// ////////////////////////////////////
    /// </summary>
    private AccountLoginDlgControl accountLoginControl = null;
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    public void WxLoginBtnClick()
    {
        Scheduling.Ins.ChangeScene(SceneType.ST_Hall);
    }

    public void AccLoginBtnClick()
    {
        CreateAccountLoginDlg();
    }


    /////////////////////////////////////
    public void AccountLogin(string acc, string pwd)
    {
        //点击了账号登录框的确认
        //TcpManager.Ins.ConnectByIpPort();
    }

    public void ExitAccountLoginDlg()
    {
        //点击了账号登录框的退出
        DestoryAccountLoginDlg();
    }

    /// <summary>
    /// 实现的接口函数
    /// </summary>
    public void OnConnectSuccess()
    {
        qp_server.qp_login_req req = new qp_server.qp_login_req();
        req.account = "yaohong";
        req.pwd = "231344781";

        byte[] buff = CmdBase.ProtoBufSerialize<qp_server.qp_login_req>(req);
        TcpManager.Ins.SendData((int)qp_server.ws_cmd.CMD_QP_LOGIN_REQ, buff);
    }
    public void OnConnectFailed()
    {

    }
    public void OnDisconnect()
    {

    }

    public void OnCompletePacket(qp_server.qp_packet packet)
    {
        switch ((qp_server.ws_cmd)packet.cmd)
        {
            case qp_server.ws_cmd.CMD_QP_LOGIN_RSP:
                OnLoginRsp(CmdBase.ProtoBufDeserialize<qp_server.qp_login_rsp>(packet.serialized));
                break;
            //case qp_server.ws_cmd.CMD_QP_CREATE_ROOM_RSP:
            //    OnCreateRoomRsp(CmdBase.ProtoBufDeserialize<qp_server.qp_create_room_rsp>(qpPacket.serialized));
            //    break;
            //case qp_server.ws_cmd.CMD_QP_JOIN_ROOM_RSP:
            //    OnJoinRoomRsp(CmdBase.ProtoBufDeserialize<qp_server.qp_join_room_rsp>(qpPacket.serialized));
            //    break;
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

    public void EnterScene()
    {
        gameObject.SetActive(true);
    }
    public void ExitScene()
    {
        //清楚场景生成的各种对象
        DestoryAccountLoginDlg();
        //隐藏自己
        gameObject.SetActive(false);
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

    void OnPingRsp(qp_server.qp_ping_rsp rsp)
    {

    }

    /// <summary>
    /// 内部函数
    /// </summary>
    void CreateAccountLoginDlg()
    {
        DestoryAccountLoginDlg();

        GameObject accountLoginDlgObj = GameObject.Instantiate(accountLoginDlgTempalte);
        AccountLoginDlgControl control = accountLoginDlgObj.GetComponent<AccountLoginDlgControl>();
        control.SetLoginControl(this);

        accountLoginDlgObj.transform.parent = dlgParentObj.transform;
        accountLoginDlgObj.transform.localScale = Vector3.one;
        accountLoginDlgObj.transform.localPosition = Vector3.one;

        accountLoginControl = accountLoginDlgObj.GetComponent<AccountLoginDlgControl>();
    }

    void DestoryAccountLoginDlg()
    {
        if (accountLoginControl != null)
        {
            GameObject.DestroyObject(accountLoginControl.gameObject);
            accountLoginControl = null;
        }
    }
}
