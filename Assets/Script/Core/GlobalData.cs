using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum LoginType {
    LT_Account,
    LT_WX,
}
public class GlobalData : MonoBehaviour {

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
    public int currentRoomId;           //当前所在的房间ID
    public int currentSeatNumber;       //当前座位号

    /// <summary>
    /// 房间里的玩家信息
    /// </summary>
    public class RoomUser
    {
        public int userId;
        public string nickname;
        public string avatarUrl;
        public int seatNumber;
    }


    public Dictionary<int, RoomUser> allRoomUsers = new Dictionary<int, RoomUser>();
    
}
