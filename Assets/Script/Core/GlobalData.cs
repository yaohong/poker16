using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum LoginType {
    LT_Account,
    LT_WX,
}

public class RoomUser
{
    public int userId;
    public string nickname;
    public string avatarUrl;
    public int seatNumber;

    public RoomUser(int tmpUserId, string tmpNickname, string tmpAvatarUrl, int tmpSeatNumber)
    {
        userId = tmpUserId;
        nickname = tmpNickname;
        avatarUrl = tmpAvatarUrl;
        seatNumber = tmpSeatNumber;
    }
}

public class RoomCfg
{
    public string roomName;                 //房间名字
    public bool isAa;                       //是不是AA制
    public int doubleDownScore;             //双下的分数
    public bool isLaiziPlaymethod;          //是不是癞子玩法
    public bool isOb;                       //是否允许观看
    public bool isRandom;                   //是否随机队友
    public bool isNotVoice;                 //是否禁音
    public bool isSafeMode;                 //安全模式
    public List<int> lockUserIdList;        //房间锁定的人

    public RoomCfg(
        string tmpRoomName, 
        bool tmpIsAa, 
        int tmpDoubleDownScore, 
        bool tmpIsLaiziPlaymethod, 
        bool tmpIsOb, 
        bool tmpIsRandom,
        bool tmpIsNotVoice,
        bool tmpIsSafeMode,
        List<int> tmpLockUserIdList
        )
    {
        roomName = tmpRoomName;
        isAa = tmpIsAa;
        doubleDownScore = tmpDoubleDownScore;
        isLaiziPlaymethod = tmpIsLaiziPlaymethod;
        isOb = tmpIsOb;
        isRandom = tmpIsRandom;
        isNotVoice = tmpIsNotVoice;
        isSafeMode = tmpIsSafeMode;
        lockUserIdList = tmpLockUserIdList;
        lockUserIdList = new List<int>();
        lockUserIdList.AddRange(lockUserIdList);
    }
}

public class GlobalData : MonoBehaviour {

    public float PING_SPACE_TIME = 10;
    static GlobalData ins = null;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void Awake()
    {
        if (ins != null)
        {
            Debug.LogError("TcpManager Recreate!!");
        }

        ins = this;
    }

    public static GlobalData Ins
    {
        get
        {
            return ins;
        }
    }
    //登陆信息
    public string serverIp = "121.41.107.94";
    public int serverPort = 18751;

    public LoginType loginType;
    public string loginAcc;
    public string loginPwd;

    //玩家自己的信息
    public int userId;
    public string nickname;
    public string avatarUrl;
    public int roomCardCount;


    /// <summary>
    /// 房间里的玩家信息
    /// </summary>
    //如果在房间的话，这里显示房间信息
    public int currentRoomId = 0;           //当前所在的房间ID
    public int currentSeatNumber = -1;       //当前座位号
    public Dictionary<int, RoomUser> allRoomUsers = new Dictionary<int, RoomUser>();
    public RoomCfg roomCfg = null;


    public void SetRoomData(qp_server.pb_room_data roomData)
    {
        qp_server.pb_room_cfg roomCfg = roomData.cfg;
        //设置房间信息
        RoomCfg cfg = new RoomCfg(
            roomCfg.room_name,
            roomCfg.is_aa,
            roomCfg.double_down_score,
            roomCfg.is_laizi_playmethod,
            roomCfg.is_ob,
            roomCfg.is_random,
            roomCfg.is_not_voice,
            roomCfg.is_safe_mode,
            roomCfg.lock_userid_list);
        GlobalData.Ins.roomCfg = cfg;
        int selfSeatNumber = -1;
        foreach (var pbRoomUser in roomData.room_users)
        {
            qp_server.pb_user_public_data roomUserPublicData = pbRoomUser.user_public_data;
            RoomUser roomUser = new RoomUser(roomUserPublicData.user_id, roomUserPublicData.nick_name, roomUserPublicData.avatar_url, pbRoomUser.seat_number);
            GlobalData.Ins.allRoomUsers.Add(roomUserPublicData.user_id, roomUser);
            if (roomUserPublicData.user_id == GlobalData.Ins.userId)
            {
                selfSeatNumber = pbRoomUser.seat_number;
            }
        }

        GlobalData.Ins.currentRoomId = roomData.room_id;
        GlobalData.Ins.currentSeatNumber = selfSeatNumber;
    }

    public void ClearRoomData()
    {
        GlobalData.Ins.roomCfg = null;
        GlobalData.Ins.allRoomUsers.Clear();
        GlobalData.Ins.currentRoomId = 0;
        GlobalData.Ins.currentSeatNumber = -1;
    }
}
