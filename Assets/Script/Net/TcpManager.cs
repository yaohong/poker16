using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;


public enum SocketTaskState
{
    STS_IDLE,
    // 闲置状态
    STS_CONNECTING,
    //连接成功
    STS_WORKING
    // 工作状态
    //STS_DISCONNECT,
    // 断线状态
}

public class TcpManager : MonoBehaviourX
{

    public static float CONNECT_TIMEOUT = 5f;       //链接的最长时间
    public static float RECEIVE_TIMEOUT = 300000f;      //30秒没收到数据

    protected TcpSocket tcpSocket = null;
    protected EndPoint serverAddr;
    public SocketTaskState socketTaskState = SocketTaskState.STS_IDLE;

    //public bool reconnectMask = false;           //重连标志
    //public int reconnectCount = 0;               //重连的次数
    //protected int autoReconnectMaxCount = 5;        //自动重连的最大次数

    public float lastReconnectTime = 0f;         //开始连接的时间
    public float lastReceivePacketTime = 0f;     //最后一次收到完整包的时间
    public float curTime = 0f;                  //当前时间
    static TcpManager ins = null;

    private Queue<IncomingPacket> msgQueue = new Queue<IncomingPacket>();

    public static TcpManager Ins
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

    void Start()
    {

    }

    void Update()
    {
        curTime = Time.time;
        switch (socketTaskState)
        {
            case SocketTaskState.STS_IDLE:
                //什么都不做
                break;
            case SocketTaskState.STS_CONNECTING:
                //套接字正在连接中
                //检测是否链接成功
                if (tcpSocket.ConncetSuccess())
                {
                    //链接成功
                    ChangeSocketTaskState(SocketTaskState.STS_WORKING);
                    lastReceivePacketTime = Time.time;
                    //判断是第一次链接成功还是重连成功
                    Log.Logic("Connect success, startConnectTime={0}, curTime={1}", lastReconnectTime, lastReceivePacketTime);
                    Eventer.Fire("ConnectSuccess", new object[] { });

                    //ResetReconnectMask(false);
                }
                else
                {
                    float now = Time.time;
                    if (now > lastReconnectTime + CONNECT_TIMEOUT)
                    {
                        //链接超时了
                        Log.Logic("Connect timeout, startConnectTime={0}, curTime={1}", lastReconnectTime, now);
                        DestroyCurrentSocket();
                        //ChangeSocketTaskState(SocketTaskState.STS_DISCONNECT);
                        ChangeSocketTaskState(SocketTaskState.STS_IDLE);
                        Eventer.Fire("ConnectFailed", new object[] {});
                    }
                }
                break;
            //case SocketTaskState.STS_DISCONNECT:
            //    if (!reconnectMask || reconnectCount >= autoReconnectMaxCount)
            //    {
            //        Log.Logic("reconnectMask={0}, reconnectCount={1}, connectFailed", reconnectMask, reconnectCount);
            //        //不支持重连，或许重连达到最大次数
            //        Eventer.Fire("ConnectFailed", new object[] { reconnectMask });
            //        //进入空闲状态
            //        ChangeSocketTaskState(SocketTaskState.STS_IDLE);
            //    }
            //    else
            //    {
            //        //进行重连
            //        float now = Time.time;
            //        tcpSocket = new TcpSocket();
            //        if (tcpSocket.Connect(serverAddr))
            //        {
            //            reconnectCount++;
            //            Log.Logic("start reconnect, reconnectMask={0}, reconnectCount={1}, time={2}", reconnectMask, reconnectCount, now);
            //            ChangeSocketTaskState(SocketTaskState.STS_CONNECTING);
            //            lastReconnectTime = Time.time;
            //        }
            //        else
            //        {
            //            Log.Logic("reconnect failed, reconnectMask={0}, reconnectCount={1}, time={2}", reconnectMask, reconnectCount, now);
            //            //直接连接失败了。不用检查重试了
            //            DestroyCurrentSocket();
            //            Eventer.Fire("ConnectFailed", new object[] { reconnectMask });
            //            ChangeSocketTaskState(SocketTaskState.STS_IDLE);
            //        }
            //    }
            //    break;
            case SocketTaskState.STS_WORKING:
                //接收数据
                
                if (!tcpSocket.DoMessageRecv(ref msgQueue))
                {
                    //读取数据失败了
                    Log.Logic("tcpSocket.DoMessageRecv failed");
                    OnDisconnect();
                }
                else
                {
                    float now = Time.time;
                    if (msgQueue.Count == 0)
                    {
                        if (now >= lastReceivePacketTime + RECEIVE_TIMEOUT)
                        {
                            Log.Logic("tcpSocket.DoMessageRecv timeout, lastReceivePacketTime={0} now={1}", lastReceivePacketTime, now);
                            //接收数据超时,当作断开处理
                            OnDisconnect();
                        }
                    }
                    else
                    {
                        //有消息
                        lastReceivePacketTime = now;
                        Log.Logic("tcpSocket.DoMessageRecv success, update lastReceivePacketTime={0}", lastReceivePacketTime);
                        foreach (IncomingPacket packet in msgQueue)
                        {
                            Eventer.Fire("CompletePacket", new object[] { packet });
                        }
                        msgQueue.Clear();
                    }
                    
                }
                break;
        }
    }


    void OnDestroy()
    {
        DestroyCurrentSocket();
        ins = null;
    }

    public bool ConnectByIpPort(string ip, int port)
    {
        DestroyCurrentSocket();
        serverAddr = new IPEndPoint(IPAddress.Parse(ip), port);
        float time = Time.time; ;
        string svrAddrString = string.Format("{0}:{1} {2}", ip, port, time);
        Log.Logic("TCP连接 [{0}]", svrAddrString);
 
        tcpSocket = new TcpSocket();
        bool connectState = tcpSocket.Connect(serverAddr);
        if (!connectState)
        {
            //链接失败了
            DestroyCurrentSocket();
            return false;
        }

        ChangeSocketTaskState(SocketTaskState.STS_CONNECTING);
        //Eventer.Fire("Connecting");
        lastReconnectTime = time;
        return true;
    }

    private void ChangeSocketTaskState(SocketTaskState newTaskState)
    {
        Log.Logic("SocketTaskState {0}=>{1}", socketTaskState, newTaskState);
        socketTaskState = newTaskState;
    }

    private void DestroyCurrentSocket()
    {
        if (tcpSocket != null)
        {
            tcpSocket.CloseSocket();
            tcpSocket = null;
            msgQueue.Clear();
        }
    }

    public void SendData(int cmd, byte[] data)
    {
        if (tcpSocket != null)
        {
            if (socketTaskState != SocketTaskState.STS_WORKING)
            {
                return;
            }

            if (!tcpSocket.SendData(cmd, data))
            {
                //发送数据失败了，销毁当前套接字
                Log.Logic("sendData faild");
                OnDisconnect();
            }
            else
            {
                Log.Logic("sendData success");
            }
        }

    }

    private void OnDisconnect()
    {
        DestroyCurrentSocket();
        ChangeSocketTaskState(SocketTaskState.STS_IDLE);
        //设置重连状态
        Eventer.Fire("Disconnect");
    }

}