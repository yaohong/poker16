using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccountLoginDlgControl : MonoBehaviour {

    private LoginControl loginControl;
    public UIInput accInput;
    public UIInput pwdInput;
    public UILabel stateLable;
	// Use this for initialization
	void Start () {
		
	}

    public void SetLoginControl(LoginControl lc)
    {
        loginControl = lc;
    }
	
	// Update is called once per frame
	void Update () {
		
	}


    public void LoginBtnClick()
    {
        if (accInput.value == "")
        {
            stateLable.text = "账号不能为空";
            return;
        }

        if (pwdInput.value == "")
        {
            stateLable.text = "密码不能为空";
            return;
        }

        loginControl.AccountLogin(accInput.value, pwdInput.value);
    }

    public void ExitBtnClick()
    {
        loginControl.ExitAccountLoginDlg();
    }
}
