using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SceneType
{
    ST_Login,                  //登陆场景
    ST_Hall,                   //大厅
    ST_Game                    //游戏界面
}

public interface IScene
{ 
    void OnConnectSuccess();
    void OnConnectFailed();
    void OnDisconnect();

    void OnCompletePacket(qp_server.qp_packet packet);

    void EnterScene();
    void ExitScene();
    
}

public class Scheduling : MonoBehaviourX
{
    public GameObject loginObject;
    public GameObject hallControl;
    public GameObject gameControl;

    static Scheduling ins = null;

    private IScene currentScene = null;
    private Dictionary<SceneType, IScene> sceneMap = new Dictionary<SceneType, IScene>();


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

	void Start () {
        sceneMap[SceneType.ST_Login] = loginObject.GetComponent<IScene>();
        sceneMap[SceneType.ST_Hall] = hallControl.GetComponent<IScene>();
        sceneMap[SceneType.ST_Game] = gameControl.GetComponent<IScene>();

        scope.Listen("ConnectSuccess", new CALLBACK(OnConnectSuccess));
        scope.Listen("ConnectFailed", new CALLBACK(OnConnectFailed));
        scope.Listen("Disconnect", new CALLBACK(OnDisconnect));
        scope.Listen("CompletePacket", new CALLBACK(OnCompletePacket));

        //UILib.SwitchProcedurePanel("LOGIN");
        ChangeScene(SceneType.ST_Login);
	}

    public void ChangeScene(SceneType type)
    {
        if (currentScene != null)
        {
            currentScene.ExitScene();
        }

        currentScene = sceneMap[type];
        currentScene.EnterScene();
    }

    void OnConnectSuccess(object []args)
    {
        Log.Logic("connect success");
        currentScene.OnConnectSuccess();
    }

    void OnConnectFailed(object[] args)
    {
        Log.Logic("connect failed");
        currentScene.OnConnectFailed();
    }

    void OnDisconnect(object[] args)
    {
        Log.Logic("disconnect");
        currentScene.OnDisconnect();
    }

    void OnCompletePacket(object[] args)
    {
        IncomingPacket packet = args[0] as IncomingPacket;
        if (packet == null)
        {
            return;
        }

        qp_server.qp_packet qpPacket =  CmdBase.ProtoBufDeserialize<qp_server.qp_packet>(packet.recvBuff);
        currentScene.OnCompletePacket(qpPacket);

    }
}
