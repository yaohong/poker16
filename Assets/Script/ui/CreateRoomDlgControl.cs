using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateRoomDlgControl : MonoBehaviour {

    public UIInput roomNameInput;
    public UIToggle cb100;
    public UIToggle cb200;
    public UIToggle cblaizi;
    public UIToggle cbob;
    public UIToggle cbrandom;
    public UIToggle cbnotvoice;
    public UIToggle cbsaft;

    private HallControl hallControl;
	// Use this for initialization

    void Start()
    {
        cb200.value = true;
        cbnotvoice.value = false;
        cbsaft.value = false;
    }

    public void SetHallControl(HallControl hc)
    {
        hallControl = hc;
    }

    //玩家AA
    public void AABtnClick()
    {
        CreateRoom(true);
    }

    //包场
    public void SelfBtnClick()
    {
        CreateRoom(false);
    }

    public void CreateRoom(bool openType)
    {
        string roomName = roomNameInput.value;
        int doubleScore = 100;
        if (cb100.value)
        {
            doubleScore = 100;
        }

        if (cb200.value)
        {
            doubleScore = 200;
        }

        hallControl.CreateRoomDlg_CreateBtnClick(roomName, doubleScore, openType, cblaizi.value, cbob.value, cbrandom.value, cbnotvoice.value, cbsaft.value);
    }

    public void ExitClick()
    {
        hallControl.CreateRoomDlg_ExitClick();
    }


}
