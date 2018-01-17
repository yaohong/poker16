using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//管理一个房间面板
public class RoomInfoControl : MonoBehaviour {

	// Use this for initialization
    private RoomViewControl viewControl = null;

    private int room_id;
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetViewControl(RoomViewControl tmpViewControl)
    {
        viewControl = tmpViewControl;
    }


    public int RoomId
    {
        get { return room_id; }
        set { room_id = value; }
    }

    public void Click()
    {
        viewControl.RoomInfoClick(this);
    }
}
