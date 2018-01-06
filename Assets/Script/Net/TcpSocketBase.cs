//using System;
//using System.Collections.Generic;
//using System.Net;
//using System.Net.Sockets;
//
//using UnityEngine;
//using System.IO;
//
//public delegate void NetCallBack(byte[] buff);
//
//public interface INetUnpackedCallBack
//{
//    INetUnpackedCallBack Unpack(byte[] buff);
//    bool Callback(bool laterProcess);
//}
//
//public class NetCallbackWithUnpacker<T> : INetUnpackedCallBack where T : ProtoBuf.IExtensible
//{
//    public delegate void NetCallBackUnpacked(T proto);
//    private NetCallBackUnpacked handler;
//
//    public delegate void NetCallBackUnpackedWithLaterTag(T proto, bool laterProcess);
//    private NetCallBackUnpackedWithLaterTag handlerWithLaterTag;
//
//    private T unpackedData;
//
//    public NetCallbackWithUnpacker(NetCallBackUnpacked callbackHandler)
//    {
//        handler = callbackHandler;
//        handlerWithLaterTag = null;
//    }
//
//    public NetCallbackWithUnpacker(NetCallBackUnpackedWithLaterTag callbackHandler)
//    {
//        handler = null;
//        handlerWithLaterTag = callbackHandler;
//    }
//
//    public INetUnpackedCallBack Unpack(byte[] buff)
//    {
//        NetCallbackWithUnpacker<T> unpacker;
//
//        if (null != handler)
//        {
//            unpacker = new NetCallbackWithUnpacker<T>(handler);
//        }
//        else if (null != handlerWithLaterTag)
//        {
//            unpacker = new NetCallbackWithUnpacker<T>(handlerWithLaterTag);
//        }
//        else
//        {
//            return null;
//        }
//
//        unpacker.unpackedData = CmdBase.ProtoBufDeserialize<T>(buff);
//
//        return unpacker;
//    }
//
//    public void SetUnpackData(T data)
//    {
//        unpackedData = data;
//    }
//
//    public bool Callback(bool laterProcess)
//    {
//        if (null == unpackedData)
//        {
//            return false;
//        }
//
//        //逻辑系统处理回调
//        if (null != handler)
//        {
//            handler(unpackedData);
//        }
//        else if (null != handlerWithLaterTag)
//        {
//            handlerWithLaterTag(unpackedData, laterProcess);
//        }
//
//        //unpackedData = default(T);
//
//        return true;
//    }
//}
//
//public enum SocketTaskState
//{
//    STS_IDLE,
//    // 闲置状态
//    STS_CONNECTING,
//    // 正在连接
//    STS_CONNECT_SUCCESS,
//    //连接成功
//    STS_WORKING,
//    // 工作状态
//    STS_DISCONNECT,
//    // 断线状态
//}
//
//public class TcpSocketBase
//{
//    protected Socket socket;
//    protected SocketTaskState state = SocketTaskState.STS_IDLE;
//    protected EndPoint endpoint;
//    protected bool reconnecting = false;
//    protected float reconnect_interval = -1f;
//    protected float last_reconnect_time = 0f;
//    protected int reconnect_times = 0;
//    protected bool sending_reconnect = false;
//    protected bool need_auto_reconnect = false;
//    protected bool force_next_reconnect = false;
//
//    protected int SOCKET_RECV_BUFF_SIZE = 128 * 1024;
//    protected int SOCKET_SEND_BUFF_SIZE = 16 * 1024;
//
//    public class IncomingPacket
//    {
//        //收到的原始二进制数据包
//        public bool isWorldProto;
//        public byte[] recvBuff = null;
//
//        public uint queueSeqId = 0;
//    }
//
//    public class IncomingProto
//    {
//        //经过外层pb解码的协议结构
//        public bool isWorldProto;
//
//        public uint cmdId;
//        public uint seqId;
//        public bool laterProto = false;
//
//        public INetUnpackedCallBack unpackedCallBack = null;
//    }
//
//    private Queue<IncomingPacket> IncomingPacketQueue = new Queue<IncomingPacket>();
//
//    //已经解包但暂未进入逻辑层处理的协议数据（场景切换帧的后续包等）
//    private Queue<IncomingProto> PendingProtoQueue;
//    private bool protoCallbackingShouldPending = false;
//
//    delegate Queue<IncomingProto> AsyncProcessIncomingPacketsDelegate(Queue<IncomingPacket> packetQueue, int maxProcessCount);
//    AsyncProcessIncomingPacketsDelegate UnpackIncomingPacketsDelegate = asyncUnpackIncomingPackets;
//    IAsyncResult arUnpackIncomingPackets;
//
//    public float ReconnectInterval
//    {
//        get { return reconnect_interval; }
//        set { reconnect_interval = value; }
//    }
//
//    public bool Connect(EndPoint ep)
//    {
//        float now = Time.time;
//        last_reconnect_time = now;
//        reconnect_times++;
//        Log.Network("Connect to:{0}, time:{1}", ep.ToString(), now);
//
//        endpoint = ep;
//        ResetPacketMakingStatus();
//        CloseSocket();
//        IncomingPacketQueue.Clear();
//        arUnpackIncomingPackets = null;
//
//        return Doconnect(ep);
//    }
//
//    protected bool Doconnect(EndPoint ep)
//    {
//        try
//        {
//            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
//            socket.NoDelay = true;
//            socket.Blocking = false;
//            socket.LingerState = new LingerOption(true, 0);
//            socket.SendBufferSize = SOCKET_SEND_BUFF_SIZE;
//            socket.ReceiveBufferSize = SOCKET_RECV_BUFF_SIZE;
//
//            state = SocketTaskState.STS_CONNECTING;
//
//            socketReceiveBuff = new byte[socket.ReceiveBufferSize];
//
//            socket.Connect(ep);
//
//            return true;
//        }
//        catch (SocketException e)
//        {
//            bool ignore = false;
//            if (e != null)
//            {
//                SocketError code = (SocketError)(e.ErrorCode);
//                if (code == SocketError.WouldBlock || code == SocketError.InProgress)
//                {
//                    ignore = true;
//                }
//                else
//                {
//                    Log.Network("ConnectException, code={0}, enum={1}", (int)code, ((Enum)code).ToString());
//                }
//            }
//
//            if (!ignore)
//            {
//                Log.Network("Connect Fail");
//                state = SocketTaskState.STS_DISCONNECT;
//                return false;
//            }
//            return true;
//        }
//    }
//
//    public static uint recvSequenceId = 0xFFFFFFFF;
//
//    public void EnqueueIncomingPacket(byte[] buff, int buffLen, short routing, bool playback = false)
//    {
//#if FALSE && UNITY_EDITOR
//        if (null != SaveProtoManager.Ins 
//            && SaveProtoManager.Ins.GetIsSaveProto() 
//            && !playback)
//        {
//            //保存协议内容
//            zbpkg.zbpkg_save_proto_info protoInfo = new zbpkg.zbpkg_save_proto_info();
//            protoInfo.save_time = Time.time;
//            protoInfo.proto_type = routing;
//            protoInfo.body_length = buffLen;
//            byte[] body = new byte[buffLen];
//            Array.Copy(buff, body, buffLen);
//            protoInfo.body = body;
//
//            if (routing == Const.SCENE_ROUTING)
//            {
//                zbpkg.zbpkg_root_rsp rsp = CmdBase.ProtoBufDeserialize<zbpkg.zbpkg_root_rsp>(buff);
//                if (rsp == null)
//                {
//                    return;
//                }
//                if (rsp.cmd != (int)zbpkg.ZBCMD.CMD_MOVE_NOTICE && rsp.cmd != (int)zbpkg.ZBCMD.CMD_PING_RSP)
//                {
//                    Eventer.Fire("SaveProto", new object[] { protoInfo });
//                }
//            }
//        }
//#endif
//
//        if (routing == Const.SCENE_ROUTING)
//        {
//#if FALSE && UNITY_EDITOR
//            if (null == SaveProtoManager.Ins
//                || (!SaveProtoManager.Ins.GetReadMode() || playback))
//#endif
//            {
//                IncomingPacket packet = new IncomingPacket();
//                packet.isWorldProto = false;
//                //packet.buffLen = buffLen;
//                packet.recvBuff = new byte[buffLen];
//                Buffer.BlockCopy(buff, 0, packet.recvBuff, 0, buffLen);
//
//                packet.queueSeqId = (uint)IncomingPacketQueue.Count;
//                IncomingPacketQueue.Enqueue(packet);
//            }
//        }
//        else
//        {
//            IncomingPacket packet = new IncomingPacket();
//            packet.isWorldProto = true;
//            //packet.buffLen = buffLen;
//            packet.recvBuff = new byte[buffLen];
//            Buffer.BlockCopy(buff, 0, packet.recvBuff, 0, buffLen);
//
//            packet.queueSeqId = (uint)IncomingPacketQueue.Count;
//            IncomingPacketQueue.Enqueue(packet);
//        }
//    }
//
//    static void UnpackOnePacketToProtoQueue(Queue<IncomingProto> protoQueue, IncomingPacket packet, int maxProcessCount)
//    {
//    }
//
//    public void AsyncUnpackIncomingPackets(int maxProcessCount)
//    {
//        int packetCount = IncomingPacketQueue.Count;
//
//        if (packetCount > 0)
//        {
//            //网络包堆积严重
//            int packetQueueLimit = maxProcessCount * 100;
//            if (packetCount > packetQueueLimit)
//            {
//                Log.Error("Too Many Packets ! Count[{0}] Warning Level[{1}]", packetCount, packetQueueLimit);
//            }
//
//            foreach (IncomingPacket packet in IncomingPacketQueue)
//            {
//                packet.queueSeqId = (uint)(packetCount - packet.queueSeqId);
//            }
//
//            arUnpackIncomingPackets = UnpackIncomingPacketsDelegate
//                .BeginInvoke(IncomingPacketQueue, maxProcessCount, null, null);
//        }
//    }
//
//    private void processCallbacks(Queue<IncomingProto> pr
//    {
//        
//    }
//
//    public void IncomingProtosCallback()
//    {
//        if (null == arUnpackIncomingPackets)
//        {
//            //本帧没有新到的协议包，也要处理掉延迟的包
//            if (null != PendingProtoQueue)
//            {
//                processCallbacks(PendingProtoQueue);
//                PendingProtoQueue = null;
//            }
//
//            return;
//        }
//
//        UnityEngine.Profiling.Profiler.BeginSample("Wait Proto Unpack");
//        Queue<IncomingProto> newProtoQueue = UnpackIncomingPacketsDelegate.EndInvoke(arUnpackIncomingPackets);
//        arUnpackIncomingPackets = null;
//        UnityEngine.Profiling.Profiler.EndSample();
//
//        if (null == PendingProtoQueue || PendingProtoQueue.Count <= 0)
//        {
//            PendingProtoQueue = null;
//            //在小蔡的电脑上newProtoQueue莫名其妙为null加上保护
//            if (newProtoQueue != null)
//            {
//                processCallbacks(newProtoQueue);
//            }
//        }
//        else
//        {
//            while (newProtoQueue.Count > 0)
//            {
//                PendingProtoQueue.Enqueue(newProtoQueue.Dequeue());
//            }
//
//            processCallbacks(PendingProtoQueue);
//        }
//    }
//
//    /*
//    public void FlushIncomingPackets()
//    {
//        //立刻在主线程上解包和处理所有网络协议
//        if (IncomingPacketQueue.Count <= 0)
//        {
//            return;
//        }
//
//        Queue<IncomingProto> protoQueue = new Queue<IncomingProto>();
//
//        while (IncomingPacketQueue.Count > 0)
//        {
//            IncomingPacket packet = IncomingPacketQueue.Dequeue();
//            UnpackOnePacketToProtoQueue(protoQueue, packet);
//        }
//
//        processCallbacks(protoQueue);
//    }*/
//
//    /*
//    protected void UnpackAndCallbackSceneProtoByLua(zbpkg.zbpkg_root_rsp rsp)
//    {
//        if (cmdCount.ContainsKey(cmdName.ToString()))
//        {
//            cmdCount[cmdName.ToString()]++;
//        }
//        else
//        {
//            cmdCount[cmdName.ToString()] = 1;
//        }
//    }*/
//
//    private byte[] socketReceiveBuff;
//
//    private const int MAX_PACKET_BODY = 64 * 1024;
//
//    //单个组包的状态
//
//    //当前包已收数据量
//    private int currentPacketReceivedHeadSize = 0;
//    private int currentPacketReceivedBodySize = 0;
//
//    private int currentPacketBodySize = 0;  //当前组包的正文部分大小
//    private short currentPacketExpand = 0;
//    private short currentPacketRoutingFlag = 0;
//    private int currentPacketHeadSeqId = 0;
//
//    private byte[] currentPacketHeadBuf = new byte[PACKET_HEAD_SIZE];
//    private byte[] currentPacketBodyBuff = new byte[MAX_PACKET_BODY];
//
//
//    const int PACKET_HEAD_SIZE = sizeof(short) + sizeof(short) + sizeof(short) + sizeof(uint);
//
//    private void ResetPacketMakingStatus()
//    {
//        currentPacketReceivedHeadSize = 0;
//        currentPacketReceivedBodySize = 0;
//
//        currentPacketBodySize = 0;
//
//        currentPacketExpand = 0;
//        currentPacketRoutingFlag = 0;
//        currentPacketHeadSeqId = 0;
//    }
//
//    private int ReadSocketReceiveBuff()
//    {
//        try
//        {
//            if (!socket.Connected)
//            {
//                OnDisconnect("Not Connected");
//                return 0;
//            }
//
//            if (socket.Poll(0, SelectMode.SelectError))
//            {
//                OnDisconnect("");
//                return 0;
//            }
//
//            int frameBytes = socket.Available;
//            if (frameBytes > 0)
//            {
//                if (frameBytes > MAX_PACKET_BODY)
//                {
//                    Debug.LogFormat("Frame Byte [{0}]", frameBytes);
//                }
//
//                if (null == socketReceiveBuff || socketReceiveBuff.Length < frameBytes)
//                {
//                    //应该不会发生
//                    socketReceiveBuff = new byte[frameBytes];
//                }
//
//                int rsize = socket.Receive(socketReceiveBuff, frameBytes, SocketFlags.None);
//                if (rsize == frameBytes)
//                {
//                    lastReceiveTick = Time.unscaledTime;
//                    return frameBytes;
//                }
//                else
//                {
//                    OnDisconnect("");
//                    return 0;
//                }
//            }
//            else
//            {
//                return 0;
//            }
//        }
//        catch (SocketException e)
//        {
//            SocketError code = (SocketError)(e.ErrorCode);
//
//            if (code != SocketError.WouldBlock && code != SocketError.Interrupted && code != SocketError.Success)
//            {
//                OnDisconnect("");
//            }
//        }
//
//        return 0;
//    }
//
//    private bool DebugIsConnected()
//    {
//        return !((socket.Poll(10, SelectMode.SelectRead) && (socket.Available == 0)) || !socket.Connected);
//    }
//
//    private bool receivingPacket = false;
//    private float lastReceiveTick = 0f;
//
//    private void DoMessageRecv()
//    {
//        int receiveBufferBytes = ReadSocketReceiveBuff();
//
//        if (receiveBufferBytes <= 0 && receivingPacket)
//        {
//            if (Time.unscaledTime - lastReceiveTick > 5f)
//            {
//                OnDisconnect("");
//                return;
//            }
//        }
//
//        while (receiveBufferBytes > 0)
//        {
//            #region 拼装组包(packet)
//            int alreadyReadBytes = 0;
//            while (alreadyReadBytes < receiveBufferBytes)
//            {
//                //接收包头
//                if (currentPacketBodySize == 0)
//                {
//                    if (currentPacketReceivedHeadSize < PACKET_HEAD_SIZE)
//                    {
//                        int recvSize = Math.Min(PACKET_HEAD_SIZE - currentPacketReceivedHeadSize, receiveBufferBytes - alreadyReadBytes);
//                        if (recvSize > 0)
//                        {
//                            Buffer.BlockCopy(socketReceiveBuff, alreadyReadBytes, currentPacketHeadBuf, currentPacketReceivedHeadSize, recvSize);
//                            alreadyReadBytes += recvSize;
//
//                            currentPacketReceivedHeadSize += recvSize;
//
//                            receivingPacket = true;
//                        }
//                        else
//                        {
//                            //无数据可收了，等下帧吧
//                            break;
//                        }
//                    }
//
//                    if (currentPacketReceivedHeadSize >= PACKET_HEAD_SIZE)
//                    {
//                        //收完一个头部，初始化收包状态
//                        ResetPacketMakingStatus();
//
//                        // 0-1, 包体长度
//                        // 2-3, 路由标记
//                        // 4-7, 序列号
//
//                        currentPacketBodySize = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(currentPacketHeadBuf, 0));
//                        currentPacketExpand = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(currentPacketHeadBuf, 2));
//                        currentPacketRoutingFlag = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(currentPacketHeadBuf, 4));
//                        currentPacketHeadSeqId = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(currentPacketHeadBuf, 6));
//
//                        //Debug.LogWarningFormat("HEAD routingFlag[{0}] @ Frame [{1}]", currentPacketRoutingFlag, Time.frameCount);
//
//                        if (currentPacketRoutingFlag != Const.GATE_ROUTING)
//                        {
//                            if (currentPacketBodySize <= 0 || currentPacketBodySize >= MAX_PACKET_BODY)
//                            {
//                                Log.Error("NetworkDataInvalid, bad packet body size [{0}] ", currentPacketBodySize);
//
//                                OnDisconnect(rt);
//                                return;
//                            }
//                        }
//                        else
//                        {88
//                            if (0 != currentPacketHeadSeqId && Const.GATE_HEAD_CTL_ID1 == currentPacketExpand)
//                            {
//                                LuaFunSet.FireEvent("ShowErrorCodeMsgBox", currentPacketHeadSeqId);
//                            }
//                            break;
//                        }
//                    }
//                }
//
//                //接收包正文
//                if (currentPacketBodySize > 0)
//                {
//                    if (currentPacketReceivedBodySize < currentPacketBodySize)
//                    {
//                        //继续接收正文
//                        int recvSize = Math.Min(currentPacketBodySize - currentPacketReceivedBodySize, receiveBufferBytes - alreadyReadBytes);
//
//                        if (recvSize > 0)
//                        {
//                            Buffer.BlockCopy(socketReceiveBuff, alreadyReadBytes, currentPacketBodyBuff, currentPacketReceivedBodySize, recvSize);
//                            alreadyReadBytes += recvSize;
//
//                            currentPacketReceivedBodySize += recvSize;
//                        }
//                        else
//                        {
//                            //本帧无数据可收了
//                            break;
//                        }
//                    }
//
//                    if (currentPacketReceivedBodySize >= currentPacketBodySize)
//                    {
//                        // 当前包正文已经收完，加入队列
//                        EnqueueIncomingPacket(currentPacketBodyBuff, currentPacketBodySize, currentPacketRoutingFlag);
//
//                        //收完一个正文，重置收包状态
//                        ResetPacketMakingStatus();
//
//                        receivingPacket = false;
//                    }
//                }
//            }
//            #endregion
//
//            if (receiveBufferBytes >= socketReceiveBuff.Length)
//            {
//                //如果本次读取的数据量达到接收缓冲区容量上限，则说明现在读缓冲区还有数据可读
//                receiveBufferBytes = ReadSocketReceiveBuff();
//            }
//            else
//            {
//                break;
//            }
//        }
//    }
//
//    private void SetWorkState()
//    {
//        Log.Network("Network Connect Success!");
//        state = SocketTaskState.STS_WORKING;
//        // CmdLogin.LoginWorldServer();
//        LuaFunSet.FireEvent("NetworkConnect");
//    }
//
//    public void RecvPackets()
//    {
//        float now = Time.time;
//        switch (state)
//        {
//            case SocketTaskState.STS_CONNECTING:
//                socket.Poll(0, SelectMode.SelectWrite);
//                if (socket.Connected)
//                {
//                    state = SocketTaskState.STS_CONNECT_SUCCESS;
//                }
//                if (ReconnectInterval > 0f && now > last_reconnect_time + ReconnectInterval)
//                {
//                    state = SocketTaskState.STS_DISCONNECT;
//                }
//
//                break;
//
//            case SocketTaskState.STS_DISCONNECT:
//                //Log.Error("{0}, {1}, {2}", now, ReconnectInterval, last_reconnect_time);
//
//                if (!NeedAutoReconnect)
//                {
//                    DisconnectAndRelogin(LuaFunSet.GetTableString("CANNOT_AUTO_RECONNECT"));
//                    return;
//                }
//
//                if (!force_next_reconnect && reconnect_times >= TcpManager.AUTO_RECONNECT_TIMES)
//                {
//                    LuaFunSet.FireEvent("NetworkTimeout");
//
//                    state = SocketTaskState.STS_IDLE;
//                    return;
//                }
//
//                if (force_next_reconnect || (ReconnectInterval > 0f &&
//                    now > last_reconnect_time + ReconnectInterval))
//                {
//                    force_next_reconnect = false;
//                    Reconnect();
//                    //Log.Error("Reconnect Res:{0}", f);
//                }
//
//                break;
//
//            case SocketTaskState.STS_WORKING:
//                if (reconnecting && !sending_reconnect)
//                {
//                    sending_reconnect = true;
//                    CmdReconnect.SendReconnectPackage();
//                }
//                DoMessageRecv();
//                break;
//
//            case SocketTaskState.STS_CONNECT_SUCCESS:
//                SetWorkState();
//                state = SocketTaskState.STS_WORKING;
//                break;
//        }
//    }
//
//    public static uint sendSequenceId = 0xFFFFFFFF;
//    private MemoryStream frameDelayedSendStream;
//
//    public static void SetSequenceStartId(uint start_id)
//    {
//        sendSequenceId = start_id;
//        recvSequenceId = start_id;
//    }
//
//    static byte[] PackageSceneData(uint cmd, byte[] data)
//    {
//        zbpkg.zbpkg_root_req req = new zbpkg.zbpkg_root_req();
//        req.cmd = cmd;
//        req.body = data;
//        req.seq_id = sendSequenceId;
//        return CmdBase.ProtoBufSerialize(req);
//    }
//
//    static byte[] PackageWorldData(uint cmd, byte[] data)
//    {
//        world_server.ws_packet_req req = new world_server.ws_packet_req();
//        req.cmd = (int)cmd;
//        req.body = data;
//        req.seq_id = sendSequenceId;
//        return CmdBase.ProtoBufSerialize(req);
//    }
//
//    byte[] PackageSendData(uint cmd, byte[] data, short routing)
//    {
//        ++sendSequenceId;
//
//        byte[] pbBuf = null;
//
//        //外层包
//        if (routing == Const.SCENE_ROUTING)
//        {
//            pbBuf = PackageSceneData(cmd, data);
//        }
//        else
//        {
//            pbBuf = PackageWorldData(cmd, data);
//        }
//
//        if (pbBuf == null)
//        {
//            return null;
//        }
//
//        //             1.包长度     2.预留    3.路由(0:Scene, 1:World)      4.协议序列号
//        uint headSize = sizeof(short) + sizeof(short) + sizeof(short) + sizeof(int);
//        uint headIndex = 0;
//
//        byte[] buff = new byte[pbBuf.Length + headSize];
//
//        byte[] lenBuf = BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)(pbBuf.Length)));
//        lenBuf.CopyTo(buff, headIndex);
//        headIndex += sizeof(short);
//
//        byte[] revBuf = BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)(0)));
//        revBuf.CopyTo(buff, headIndex);
//        headIndex += sizeof(short);
//
//        byte[] rouBuf = BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)(routing)));
//        rouBuf.CopyTo(buff, headIndex);
//        headIndex += sizeof(short);
//
//        byte[] reqBuf = BitConverter.GetBytes((int)IPAddress.HostToNetworkOrder((int)sendSequenceId));
//        reqBuf.CopyTo(buff, headIndex);
//        headIndex += sizeof(int);
//
//        pbBuf.CopyTo(buff, headSize);
//
//        return buff;
//    }
//
//    public int SendData(uint cmd, byte[] data, short routing)
//    {
//        if (state != SocketTaskState.STS_WORKING)
//        {
//            return -1;
//        }
//
//        try
//        {
//            if (!socket.Connected)
//            {
//                OnDisconnect("Not Connected");
//                return 0;
//            }
//
//            if (socket.Poll(0, SelectMode.SelectError))
//            {
//                string rt = string.Format("{0}:{1}",
//                    LuaFunSet.GetTableString("SOCKET_ERROR") + "_S",
//                    Const.SOCKET_READ_POOL_SELECTERROR);
//                OnDisconnect(rt);
//                return 0;
//            }
//
//            if (routing == Const.SCENE_ROUTING)
//            {
//                Log.Network("[SEND S] cmd[{1}] ({2}) seqId[{0}]", sendSequenceId, ((zbpkg.ZBCMD)cmd).ToString(), cmd);
//            }
//            else
//            {
//                Log.Network("[SEND W] cmd[{1}] ({2}) seqId[{0}]", sendSequenceId, ((world_server.ws_cmd)cmd).ToString(), cmd);
//            }
//
//            byte[] sendBuff = PackageSendData(cmd, data, routing);
//            if (sendBuff == null)
//            {
//                return 1;
//            }
//
//            if (null == frameDelayedSendStream && socket.Poll(0, SelectMode.SelectWrite))
//            {
//                //可以立刻发送，且之前没有单帧上行堆积，就立刻发送
//                int sendSize = socket.Send(sendBuff, SocketFlags.None);
//                if (sendSize != sendBuff.Length)
//                {
//                    Log.Error("Socket Error:BAD_SEND should[{0}] got[{1}] cmd[{2}]", sendBuff.Length, sendSize, cmd);
//                    string rt = string.Format("{0}:{1},{2}",
//                        LuaFunSet.GetTableString("SOCKET_ERROR"),
//                        Const.SOCKET_SEND_LENGTH_NOTMATCH, cmd);
//                    OnDisconnect(rt);
//                    return 2;
//                }
//
//                ++totalStatics.SendPackets;
//                totalStatics.SendBytes += (uint)sendSize;
//            }
//            else
//            {
//                //无法实时发送出去的数据，延迟到帧更新的时候再法
//                if (null == frameDelayedSendStream)
//                {
//                    frameDelayedSendStream = new MemoryStream(sendBuff.Length);
//                }
//                frameDelayedSendStream.Write(sendBuff, 0, sendBuff.Length);
//
//                ++totalStatics.SendPackets;
//
//                return 0;
//            }
//        }
//        catch (SocketException e)
//        {
//            if (e != null)
//            {
//                if ((int)SocketError.WouldBlock != e.ErrorCode
//                    && (int)SocketError.InProgress != e.ErrorCode
//                    && (int)SocketError.Interrupted != e.ErrorCode)
//                {
//                    string rt = string.Format("{0}:{1},{2}",
//                        LuaFunSet.GetTableString("SOCKET_ERROR"),
//                        Const.SOCKET_SEND_EXCEPTION, e.ErrorCode);
//                    OnDisconnect(rt);
//                    return 3;
//                }
//                else
//                {
//                    Debug.LogException(e);
//                }
//            }
//        }
//
//        //Log.Error("SendOK,cmd:{0},routing:{1}", cmd, routing);
//        return 0;
//    }
//
//    public bool SendDelayedData()
//    {
//        if (null == frameDelayedSendStream)
//        {
//            return false;
//        }
//
//        try
//        {
//            if (socket.Poll(0, SelectMode.SelectWrite))
//            {
//                byte[] delayedData = new byte[frameDelayedSendStream.Length];
//                frameDelayedSendStream.Seek(0, SeekOrigin.Begin);
//                frameDelayedSendStream.Read(delayedData, 0, (int)frameDelayedSendStream.Length);
//
//                frameDelayedSendStream.SetLength(0);
//                frameDelayedSendStream = null;
//
//                int sendSize = socket.Send(delayedData, SocketFlags.None);
//                if (sendSize == delayedData.Length)
//                {
//                    //成功补发
//                    totalStatics.SendBytes += (uint)delayedData.Length;
//
//                    Debug.LogFormat("Delayed Send [{0}]", sendSize);
//
//                    return true;
//                }
//                else
//                {
//                    Log.Error("[SendDelayedData] SEND ERROR: should[{0}] got[{1}]", delayedData.Length, sendSize);
//
//                    string rt = string.Format("{0}:{1} DD",
//                        LuaFunSet.GetTableString("SOCKET_ERROR"),
//                        Const.SOCKET_SEND_LENGTH_NOTMATCH);
//                    OnDisconnect(rt);
//
//                    return false;
//                }
//            }
//            else
//            {
//                Debug.LogFormat("[SendDelayedData] still CANNOT SEND");
//                return false;
//            }
//        }
//        catch (SocketException e)
//        {
//            if (e != null)
//            {
//                if ((int)SocketError.WouldBlock != e.ErrorCode
//                    && (int)SocketError.InProgress != e.ErrorCode
//                    && (int)SocketError.Interrupted != e.ErrorCode)
//                {
//                    string rt = string.Format("{0}:{1},{2} DD",
//                        LuaFunSet.GetTableString("SOCKET_ERROR"),
//                        Const.SOCKET_SEND_EXCEPTION, e.ErrorCode);
//                    OnDisconnect(rt);
//                    return false;
//                }
//                else
//                {
//                    Debug.LogException(e);
//                }
//            }
//        }
//
//        return false;
//    }
//
//    public void OnDisconnect(string rt)
//    {
//        if (state == SocketTaskState.STS_WORKING)
//        {
//            state = SocketTaskState.STS_DISCONNECT;
//
//            Log.Error("OnDisconnect:{0}", rt);
//
//            GameManager.Ins.ClearMapAllObject();
//            Eventer.Fire("NetworkDisconnect");
//            LuaFunSet.FireEvent("NetworkDisconnect", rt);
//
//            TcpManager.Ins.ClearAllTimeoutMap();
//            //断线都清除所有积存的协议数据
//            IncomingPacketQueue.Clear();
//
//            if (null != PendingProtoQueue)
//            {
//                PendingProtoQueue.Clear();
//            }
//        }
//    }
//
//    public void DisconnectAndRelogin(string rt)
//    {
//        if (socket != null && socket.Connected)
//        {
//            socket.Disconnect(true);
//        }
//
//        Log.Error("DisconnectAndRelogin:{0}", rt);
//        state = SocketTaskState.STS_IDLE;
//        Eventer.Fire("NetworkNeedRelogin");
//        LuaFunSet.FireEvent("NetworkNeedRelogin", rt);
//
//        AccountManager.Ins.Reset();
//    }
//
//    public bool Reconnect()
//    {
//        //Log.Error("Reconnect:{0}", reconnect_times);
//        if (Connect(endpoint))
//        {
//            reconnecting = true;
//            sending_reconnect = false;
//            return true;
//        }
//        return false;
//    }
//
//    public void SetReconnected()
//    {
//        reconnecting = false;
//        sending_reconnect = false;
//    }
//
//    public void CloseSocket()
//    {
//        if (socket != null)
//        {
//            socket.Close();
//            socket = null;
//        }
//    }
//
//    public void DelayLaterProtoCallback()
//    {
//        //停止后续协议的Callback，等待下一帧（下一帧开始前需要切换scene）
//        protoCallbackingShouldPending = true;
//    }
//
//    public bool IsWorkState()
//    {
//        return state == SocketTaskState.STS_WORKING;
//    }
//
//    public SocketTaskState State
//    {
//        get { return state; }
//        set { state = value; }
//    }
//
//    public void ResetReconnectTimes()
//    {
//        reconnect_times = 0;
//    }
//
//    public void ForceNextReconnect()
//    {
//        force_next_reconnect = true;
//    }
//
//    public bool NeedAutoReconnect
//    {
//        get { return need_auto_reconnect; }
//        set { need_auto_reconnect = value; }
//    }
//}