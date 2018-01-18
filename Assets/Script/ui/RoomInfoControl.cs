using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//管理一个房间面板
public enum ERoomState {
    RS_Ready = 0,           //准备中
    RS_Game = 1,            //游戏中
}
public class RoomInfoControl : MonoBehaviour {

	// Use this for initialization
    private RoomViewControl viewControl = null;
    private ERoomState roomState = ERoomState.RS_Ready;

    public UISprite stateSprite = null;
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

    public ERoomState  RoomState
    {
        get { return roomState; }
        set
        {
            if (value == ERoomState.RS_Game)
            {
                stateSprite.spriteName = "已开始@2x";
            }
            else
            {
                stateSprite.spriteName = "准备中@2x";
            }

            roomState = value;
        }
    }

    public void Click()
    {
        viewControl.RoomInfoClick(this);
    }
}
