using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using UnityEngine;
using System.IO;

public class IncomingPacket
{
    //收到的原始二进制数据包
    public byte[] recvBuff = null;
}
public class TcpSocket
{
    protected Socket socket;
    //protected SocketTaskState state = SocketTaskState.STS_IDLE;
    //protected EndPoint endpoint;
    //protected bool reconnecting = false;
    //protected float reconnect_interval = -1f;
    //protected float last_reconnect_time = 0f;
    //protected int reconnect_times = 0;
    //protected bool sending_reconnect = false;
    //protected bool need_auto_reconnect = false;
   // protected bool force_next_reconnect = false;

    protected int SOCKET_RECV_BUFF_SIZE = 128 * 1024;
    protected int SOCKET_SEND_BUFF_SIZE = 128 * 1024;

    //已经解包但暂未进入逻辑层处理的协议数据（场景切换帧的后续包等）
    public bool Connect(EndPoint ep)
    {
        float now = Time.time;
        Log.Network("Connect to:{0}, time:{1}", ep.ToString(), now);
        ResetPacketMakingStatus();

        socketReceiveBuff = new byte[SOCKET_RECV_BUFF_SIZE];
        
        return Doconnect(ep);
    }

    protected bool Doconnect(EndPoint ep)
    {
        try
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.NoDelay = true;
            socket.Blocking = false;
            socket.LingerState = new LingerOption(true, 0);
            socket.SendBufferSize = SOCKET_SEND_BUFF_SIZE;
            socket.ReceiveBufferSize = SOCKET_RECV_BUFF_SIZE;

            socket.Connect(ep);
            return true;
        }
        catch (SocketException e)
        {
            bool ignore = false;
            SocketError code = (SocketError)(e.ErrorCode);
            if (code == SocketError.WouldBlock || code == SocketError.InProgress)
            {
                ignore = true;
            }
            else
            {
                Log.Network("ConnectException, code={0}, enum={1}", (int)code, ((Enum)code).ToString());
            }

            if (!ignore)
            {
                Log.Network("Connect Fail");
                return false;
            }
            return true;
        }
    }

    private byte[] socketReceiveBuff;

    private const int PACKET_HEAD_SIZE = sizeof(int);
    private const int MAX_PACKET_BODY = 4 * 1024;

    //当前包已收数据量
    private int currentPacketReceivedHeadSize = 0;              //已经读头的值
    private int currentPacketReceivedBodySize = 0;              //已经读body部分的值

    private int currentPacketBodySize = 0;                      //当前组包的正文部分大小

    private byte[] currentPacketHeadBuf = new byte[PACKET_HEAD_SIZE];
    private byte[] currentPacketBodyBuff = new byte[MAX_PACKET_BODY];

    private void ResetPacketMakingStatus()
    {
        currentPacketReceivedHeadSize = 0;
        currentPacketReceivedBodySize = 0;

        currentPacketBodySize = 0;

    }

    private int ReadSocketReceiveBuff()
    {
        try
        {
            //没有链接
            if (!socket.Connected)
            {
                Log.Network("ReadSocketReceiveBuff failed, socket.Connected=false");
                return -1;
            }

            //套接字出错了
            if (socket.Poll(0, SelectMode.SelectError))
            {
                Log.Network("ReadSocketReceiveBuff failed, socket.Poll(0, SelectMode.SelectError)=true");
                return -1;
            }

            int frameBytes = socket.Available;
            if (frameBytes > 0)
            {
                int rsize = socket.Receive(socketReceiveBuff, frameBytes, SocketFlags.None);
                if (rsize == frameBytes)
                {
                    return frameBytes;
                }
                else
                {
                    //应该不会发生
                    Log.Network("ReadSocketReceiveBuff failed, socket.Available={0} receiveSize={1}", frameBytes, rsize);
                    return -1;
                }
            }
            else
            {
                //没有数据读
                return 0;
            }
        }
        catch (SocketException e)
        {
            SocketError code = (SocketError)(e.ErrorCode);

            if (code != SocketError.WouldBlock && code != SocketError.Interrupted && code != SocketError.Success)
            {
                Log.Network("RecvDataException Disconnect, code={0}, enum={1}", (int)code, code.ToString());
                return -1;
            }

            return 0;
        }
    }

    public bool DoMessageRecv(ref Queue<IncomingPacket> IncomingPacketQueue)
    {
        int receiveBufferBytes = ReadSocketReceiveBuff();
        if (0 == receiveBufferBytes)
        {
            //没有数据读
            return true;
        }

        if (-1 == receiveBufferBytes)
        {
            //套接字出错了
            return false;
        }

        #region 拼装组包(packet)
        int alreadyReadBytes = 0;
        while (alreadyReadBytes < receiveBufferBytes)
        {
            //接收包头
            if (currentPacketBodySize == 0)
            {
                if (currentPacketReceivedHeadSize < PACKET_HEAD_SIZE)
                {
                    int recvSize = Math.Min(PACKET_HEAD_SIZE - currentPacketReceivedHeadSize, receiveBufferBytes - alreadyReadBytes);
                    if (recvSize > 0)
                    {
                        Buffer.BlockCopy(socketReceiveBuff, alreadyReadBytes, currentPacketHeadBuf, currentPacketReceivedHeadSize, recvSize);
                        alreadyReadBytes += recvSize;
                        currentPacketReceivedHeadSize += recvSize;
                    }
                    else
                    {
                        //无数据可收了，等下帧吧
                        break;
                    }
                }

                if (currentPacketReceivedHeadSize == PACKET_HEAD_SIZE)
                {
                    //收完一个头部，初始化收包状态
                    ResetPacketMakingStatus();
                    // 0-3, 包体长度
                    currentPacketBodySize = (int)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(currentPacketHeadBuf, 0));
                    if (currentPacketBodySize <= 0 || currentPacketBodySize >= MAX_PACKET_BODY)
                    {
                        //body长度异常
                        Log.Error("NetworkDataInvalid, bad packet body size [{0}] ", currentPacketBodySize);
                        return false;
                    }
                }
                else
                {
                    Log.Error("currentPacketReceivedHeadSize={0} Invalid", currentPacketReceivedHeadSize);
                    return false;
                }
            }

            //接收包正文
            if (currentPacketBodySize > 0)
            {
                if (currentPacketReceivedBodySize < currentPacketBodySize)
                {
                    //继续接收正文
                    int recvSize = Math.Min(currentPacketBodySize - currentPacketReceivedBodySize, receiveBufferBytes - alreadyReadBytes);

                    if (recvSize > 0)
                    {
                        Buffer.BlockCopy(socketReceiveBuff, alreadyReadBytes, currentPacketBodyBuff, currentPacketReceivedBodySize, recvSize);
                        alreadyReadBytes += recvSize;
                        currentPacketReceivedBodySize += recvSize;
                    }
                    else
                    {
                        //本帧无数据可收了
                        break;
                    }
                }

                if (currentPacketReceivedBodySize == currentPacketBodySize)
                {
                    // 当前包正文已经收完，加入队列
                    IncomingPacket packet = new IncomingPacket();
                    packet.recvBuff = new byte[currentPacketBodySize];
                    Buffer.BlockCopy(currentPacketBodyBuff, 0, packet.recvBuff, 0, currentPacketBodySize);
                    IncomingPacketQueue.Enqueue(packet);
                    //收完一个正文，重置收包状态
                    ResetPacketMakingStatus();
                } 
                else
                {
                    Log.Error("currentPacketReceivedBodySize={0} Invalid", currentPacketReceivedBodySize);
                    return false;
                }
            }
        }
        #endregion

        return true;
    }

    byte[] PackageSendData(int cmd, byte[] data)
    {

        qp_server.qp_packet req = new qp_server.qp_packet();
        req.cmd = cmd;
        req.serialized = data;

        byte[] pbBuf = CmdBase.ProtoBufSerialize(req);

        //外层包

        if (pbBuf == null)
        {
            return null;
        }

        //             1.包长度 
        uint headSize = sizeof(int);
        uint headIndex = 0;

        byte[] buff = new byte[pbBuf.Length + headSize];

        byte[] lenBuf = BitConverter.GetBytes((int)IPAddress.HostToNetworkOrder((int)(pbBuf.Length)));
        lenBuf.CopyTo(buff, headIndex);
        headIndex += headSize;

        pbBuf.CopyTo(buff, headIndex);

        return buff;
    }

    public bool SendData(int cmd, byte[] data)
    {
        try
        {
            if (!socket.Connected)
            {
                Log.Error("SendData failed, socket.Connected = false ");
                return false;
            }

            if (socket.Poll(0, SelectMode.SelectError))
            {
                Log.Error("SendData failed, socket.Poll(0, SelectMode.SelectError)=true");
                return false;
            }

            byte[] sendBuff = PackageSendData(cmd, data);
            if (sendBuff == null)
            {
                Log.Error("PackageSendData failed");
                return false;
            }

            if (socket.Poll(0, SelectMode.SelectWrite))
            {
                //可以立刻发送，且之前没有单帧上行堆积，就立刻发送
                int sendSize = socket.Send(sendBuff, SocketFlags.None);
                if (sendSize != sendBuff.Length)
                {
                    Log.Error("Socket Error:BAD_SEND should[{0}] got[{1}] cmd[{2}]", sendBuff.Length, sendSize, cmd);
                    return false;
                }
                return true;
            }
            else
            {
                Log.Error("SendData failed, socket.Poll(0, SelectMode.SelectWrite)=false");
                return false;
            }
        }
        catch (SocketException e)
        {
            SocketError code = (SocketError)e.ErrorCode;
            Log.Network("RecvDataException Disconnect, code={0}, enum={1}", (int)code, code.ToString());
            return false;
        }
    }

    public void CloseSocket()
    {
        if (socket != null)
        {
            socket.Close();
            socket = null;
        }
    }

    public bool ConncetSuccess()
    {
        socket.Poll(0, SelectMode.SelectWrite);
        return socket.Connected;
    }

}