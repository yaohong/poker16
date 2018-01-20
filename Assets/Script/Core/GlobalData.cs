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

    public string serverIp = "121.41.107.94";
    public int serverPort = 18751;

    public LoginType loginType;
    public string loginAcc;
    public string loginPwd;
}
