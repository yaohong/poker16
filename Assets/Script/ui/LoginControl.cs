using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LoginControl : MonoBehaviourX, IScene
{

    //checkbox
	// Use this for initialization
    public GameObject accountLoginDlgTempalte;
    public GameObject dlgParentObj;

    /// <summary>
    /// ////////////////////////////////////
    /// </summary>
    private AccountLoginDlgControl accountLoginControl = null;
    private BlockedControl blockedControl = null;
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    public void WxLoginBtnClick()
    {
        Scheduling.Ins.ChangeScene(SceneType.ST_Hall);
    }

    //账号登陆面板的回调请求
    public void AccLoginBtnClick()
    {
        CreateAccountLoginDlg();
    }

    /////////////////////////////////////
    //账号登录框点击等级
    public void AccountLogin(string acc, string pwd)
    {
        //点击了账号登录框的确认

        if (!TcpManager.Ins.ConnectByIpPort(GlobalData.Ins.serverIp, GlobalData.Ins.serverPort))
        {
            accountLoginControl.SetState("链接服务器失败");
            return;
        }

        //保存登陆信息
        GlobalData.Ins.loginType = LoginType.LT_Account;
        GlobalData.Ins.loginAcc = acc;
        GlobalData.Ins.loginPwd = pwd;

        //弹出遮挡板
        blockedControl = Common.Ins.CreateBlocked(dlgParentObj);
        blockedControl.SetState("正在链接服务器");
    }

    //账号登录框点击退出
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
        blockedControl.SetState("链接服务器成功,验证账号密码");
        NetPacketHandle.SendLoginReq();
    }
    public void OnConnectFailed()
    {
        DestoryBlocked();
        if (accountLoginControl != null)
        {
            accountLoginControl.SetState("链接服务器失败");
        }
        
        
    }
    public void OnDisconnect()
    {
        Log.Logic("scene[login], disconnect");
        DestoryBlocked();
        if (accountLoginControl != null)
        {
            accountLoginControl.SetState("服务器断开链接");
        }
        
    }

    public void OnCompletePacket(qp_server.qp_packet packet)
    {
        switch ((qp_server.ws_cmd)packet.cmd)
        {
            case qp_server.ws_cmd.CMD_QP_LOGIN_RSP:
                OnLoginRsp(CmdBase.ProtoBufDeserialize<qp_server.qp_login_rsp>(packet.serialized));
                break;
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
        //清除场景生成的各种对象(销毁_MESSAGEBOX下的所有对象)
        DestoryAccountLoginDlg();
        DestoryBlocked();
        //隐藏自己
        gameObject.SetActive(false);
    }

   

    void OnLoginRsp(qp_server.qp_login_rsp rsp)
    {
        Log.Logic("login_rsp state={0}",rsp.state);
        if (rsp.state != 0)
        {
            DestoryBlocked();
            if (accountLoginControl != null)
            {
                accountLoginControl.SetState("账号验证失败");
            }
        }
        else
        {
            DestoryBlocked();
            NetPacketHandle.LoginRspHandle(rsp);
        }

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

    void DestoryBlocked()
    {
        if (blockedControl != null)
        {
            GameObject.DestroyObject(blockedControl.gameObject);
            blockedControl = null;
        }
    }
}
