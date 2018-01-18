using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginControl : MonoBehaviourX
{
    public UIToggle toggle;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    public void WxLoginBtnClick()
    {

    }

    public void AccLoginBtnClick()
    {
        Log.Logic("{0}", toggle.isActiveAndEnabled);
        if (!TcpManager.Ins.ConnectByIpPort("121.41.107.94", 18751))
        {
            //链接服务器失败
            Log.Error("ConnectByIpPort failed.");
            return;
        }



    }
}
