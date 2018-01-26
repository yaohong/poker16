using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumberBtnControl : MonoBehaviour {

    public int number;
	// Use this for initialization
    private JoinRoomDlgControl parentDlgControl;
	void Start () {
        parentDlgControl = gameObject.transform.parent.GetComponent<JoinRoomDlgControl>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnClick()
    {
        parentDlgControl.NumberBtnClick(number);
    }
}
