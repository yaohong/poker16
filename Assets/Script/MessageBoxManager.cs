using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void MSGBOXCALLBACK();

public class MessageBoxManager : MonoBehaviourX 
{
	public GameObject dialogTempalte;

	void CallMsgCB(object obj)
	{
		if (obj != null && obj is MSGBOXCALLBACK)
		{
			MSGBOXCALLBACK cb = (MSGBOXCALLBACK)obj;
			cb();
		}
	}

	void Start () 
	{
		scope.Listen ("ShowMessageBox", delegate(object[] args) 
		{
			if ( args.Length > 0 )
			{
				GameObject newDialog = GameObject.Instantiate(dialogTempalte);
				newDialog.transform.parent = this.gameObject.transform;
				newDialog.transform.localScale = Vector3.one;
				newDialog.transform.localPosition = Vector3.zero;
				
				GameObject MessageObj = newDialog.FindChildByName("Message");
				MessageObj.GetComponent<UILabel>().text = args[0].ToString();

				UIEventListener.Get(newDialog.FindChildByName("OK")).onClick = delegate(GameObject go) 
				{
					GameObject.Destroy(newDialog);
					if (args.Length >= 1)
					{
						CallMsgCB(args[1]);
					}
				}; 

				UIEventListener.Get(newDialog.FindChildByName("Cancel")).onClick = delegate(GameObject go) 
				{
					GameObject.Destroy(newDialog);
					if (args.Length>=2)
					{
						CallMsgCB(args[2]);
					}
				}; 
			}
			else
			{
				Debug.LogError("message args invalid!");
			}
		});
	}
}
