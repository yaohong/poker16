//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Net;
//using UnityEngine;
//
//public class TcpManager : MonoBehaviourX
//{
//    public static int SEND_BUFFSIZE = 8192;
//    public static int RECEIVE_BUFFSIZE = 8192;
//    public static int TIMEOUT = 8000;
//    public static int CONNECT_RETRY_TIMES = 5000;
//    public static int CONNECT_INTERVAL_MILLSEC = 1000;
//
//    public static int INCOMING_PROTO_PROCESS_PRE_SECOND = 2000;
//    public static float RECONNECT_TIMEOUT = 5f;
//    public static int AUTO_RECONNECT_TIMES = 2;
//
//    public static float REQUEST_TIMEOUT = 11f;
//    public static int REQUEST_MINTIMES = 30;
//
//    public class TimeoutCfg
//    {
//        public float tickPoint;
//        public int times;
//        public bool isOpen;
//        public short routing;
//        public uint cmdId;
//
//        public TimeoutCfg(float _tickPoint = 0f, int _times = 0, bool _isOpen = true)
//        {
//            tickPoint = _tickPoint;
//            times = _times;
//            isOpen = _isOpen;
//        }
//    }
//
//    protected TcpSocketBase tcpSocket;
//
//	protected Dictionary<int, INetUnpackedCallBack> sceneUnpackedCallBackMap = new Dictionary<int, INetUnpackedCallBack>();
//
//    protected string accountId = "";
//    protected long worldToken = 0;
//    protected TimeoutCfg timeoutCfg = new TimeoutCfg(0f, 0, false);
//
//    public string AccountId
//    {
//        set { accountId = value; }
//        get { return accountId; }
//    }
//
//    public long WorldToken
//    {
//        set { worldToken = value; }
//        get { return worldToken; }
//    }
//
//    protected bool isReconnectRequest = false;
//
//    public bool IsReconnectRequest
//    {
//        get { return isReconnectRequest; }
//        set { isReconnectRequest = value; }
//    }
//
//    static TcpManager ins = null;
//
//    public static TcpManager Ins
//    {
//        get
//        {
//            return ins;
//        }
//    }
//
//    private void Awake()
//    {
//        if (ins != null)
//        {
//            Debug.LogError("TcpManager Recreate!!");
//        }
//
//        ins = this;
//    }
//
//    void Start()
//    {
//        //SceneCmdLogin.RegisterLoginScene();
//
//        scope.Listen("NetworkPlayerReconnect", (args) =>
//        {
//            if (tcpSocket != null)
//            {
//                tcpSocket.ForceNextReconnect();
//                tcpSocket.State = SocketTaskState.STS_DISCONNECT;
//            }
//        });
//
//        scope.Listen("GameShutdown", (args) =>
//        {
//            accountId = "";
//            worldToken = 0;
//            isReconnectRequest = false;
//            timeoutCfg = new TimeoutCfg(0f, 0, false);
//            if (tcpSocket != null)
//            {
//                tcpSocket.ResetReconnectTimes();
//                tcpSocket.NeedAutoReconnect = false;
//            }
//        });
//
//        scope.Listen("NetworkShutDownBeforeGameReload", args =>
//        {
//            string reason = (string) args[0];
//            // DisconnectAndRelogin(reason);
//        });
//    }
//
//    void Update()
//    {
//        if (tcpSocket != null)
//        {
//            tcpSocket.RecvPackets();
//
//            float frameProcessPacketsLimit = INCOMING_PROTO_PROCESS_PRE_SECOND * Time.deltaTime;
//            tcpSocket.AsyncUnpackIncomingPackets(Math.Max((int)frameProcessPacketsLimit, 100));
//			if (tcpSocket.State != SocketTaskState.STS_WORKING)
//            {
//                return;
//            }
//
//            tcpSocket.SendDelayedData();
//
//            if (timeoutCfg.isOpen)
//            {
//                float now = Time.unscaledTime;
//                if (now >= timeoutCfg.tickPoint && timeoutCfg.times >= REQUEST_MINTIMES)
//                {
//                    timeoutCfg.isOpen = false;
//                    tcpSocket.OnDisconnect("");
//                    if (IsReconnectRequest || !tcpSocket.NeedAutoReconnect)
//                    {
//                        tcpSocket.DisconnectAndRelogin(rt);
//                    }
//                }
//                timeoutCfg.times++;
//            }
//        }
//    }
//
//    public void IncomingProtosCallback()
//    {
//        //从GameManager延迟调用
//        if (tcpSocket != null)
//        {
//            UnityEngine.Profiling.Profiler.BeginSample("Proto Callbacks");
//            tcpSocket.IncomingProtosCallback();
//            UnityEngine.Profiling.Profiler.EndSample();
//        }
//    }
//
//    new void OnDestroy()
//    {
//        if (tcpSocket != null)
//        {
//            tcpSocket.CloseSocket();
//        }
//        ins = null;
//    }
//
//    public void ConnectByIpPort(string ip, int port)
//    {
//        EndPoint ServerAddr = new IPEndPoint(IPAddress.Parse(ip), port);
//
//        if (tcpSocket != null)
//        {
//            tcpSocket.CloseSocket();
//        }
//
//        tcpSocket = new TcpSocketBase();
//        tcpSocket.ReconnectInterval = RECONNECT_TIMEOUT;
//        tcpSocket.Connect(ServerAddr);
//
//        string svrAddrString = string.Format("{0}:{1}", ip, port);
//        Log.Logic("TCP连接 [{0}]", svrAddrString);
//        Eventer.Fire("BUGREPORT_CONNECT", new object[] { svrAddrString });
//    }
//
//    protected bool removeTimeoutCfg()
//    {
//        if (timeoutCfg.isOpen)
//        {
//            timeoutCfg.isOpen = false;
//            return true;
//        }
//        return false;
//    }
//
//
//    public INetUnpackedCallBack ScenePacketUnpack(int cmd, byte[] body)
//    {
//        INetUnpackedCallBack unpackedCallbackFunc;
//        if (sceneUnpackedCallBackMap.TryGetValue(cmd, out unpackedCallbackFunc))
//        {
//            return unpackedCallbackFunc.Unpack(body);
//        }
//        else
//        {
//#if UNITY_EDITOR
//            Log.Warning("Callback NOT Registered for scene proto [{0}]", cmd.ToString());
//#endif
//            return null;
//        }
//    }
//
//    public void RegisterUnpackedCallBack(int cmd, INetUnpackedCallBack callback)
//    {
//        sceneUnpackedCallBackMap.Add(cmd, callback);
//    }
//
//
//    public void ClearCallBack()
//    {
//        sceneUnpackedCallBackMap.Clear();
//    }
//
//    protected void updateSender(short routing, uint cmdId)
//    {
//        if (timeoutCfg.isOpen)
//        {
//            return;
//        }
//
//        timeoutCfg.isOpen = true;
//        timeoutCfg.tickPoint = Time.unscaledTime + REQUEST_TIMEOUT;
//        timeoutCfg.times = 0;
//        timeoutCfg.routing = routing;
//        timeoutCfg.cmdId = cmdId;
//    }
//
//    private bool _SendData(uint cmd, byte[] data, short routing)
//    {
//        if (tcpSocket == null)
//        {
//            return false;
//        }
//
//        int rt = tcpSocket.SendData(cmd, data, routing);
//        if (rt == 0)
//        {
//            return true;
//        }
//
//        if (rt > 0)
//        {
//            Log.Error("SendTcpDataFail, cmdId={0}, result={1}", cmd.ToString(), rt);
//        }
//
//        return false;
//    }
//
//
//    public bool SendScene(int cmd, byte[] data)
//    {
//        bool ret = _SendData((uint)cmd, data, Const.SCENE_ROUTING);
//        if (ret) updateSender(Const.SCENE_ROUTING, (uint)cmd);
//        return ret;
//    }
//
//    public void ClearAllTimeoutMap()
//    {
//        timeoutCfg.isOpen = false;
//    }
//
//    public void SetReconnected()
//    {
//        if (tcpSocket != null)
//        {
//            tcpSocket.SetReconnected();
//        }
//    }
//
//    public void DelayLaterProtoCallback()
//    {
//        if (tcpSocket != null)
//        {
//            tcpSocket.DelayLaterProtoCallback();
//        }
//    }
//
//    public bool IsWorkState()
//    {
//        return tcpSocket.IsWorkState();
//    }
//
//    public void Disconnect(string rt)
//    {
//        tcpSocket.OnDisconnect(rt);
//    }
//
//    public void DisconnectAndRelogin(string rt)
//    {
//        tcpSocket.DisconnectAndRelogin(rt);
//    }
//
//    public bool NeedPing
//    {
//        get { return tcpSocket.State == SocketTaskState.STS_WORKING; }
//    }
//
//    public void DoEnterScene()
//    {
//        IsReconnectRequest = false;
//        if (tcpSocket != null)
//        {
//            tcpSocket.ResetReconnectTimes();
//            tcpSocket.NeedAutoReconnect = true;
//        }
//    }
//}