using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginLogic : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnOK()
	{
	}

	void OnCancel()
	{
		
	}

	public void OnLoginSuccess()
	{
		Debug.Log ("AAAA");
		UILib.SwitchProcedurePanel ("DESKTOP");

//		MSGBOXCALLBACK okcb = delegate() 
//		{
//			Debug.LogError("ok");
//		};
//
//		MSGBOXCALLBACK cancelcb = delegate() 
//		{
//			Debug.LogError("cancel");
//		};
//
//		Eventer.Fire("ShowMessageBox", new object[]　
//		{
//				"哈哈哈", 
//				okcb,
//				cancelcb,
//		} );
	}
}
