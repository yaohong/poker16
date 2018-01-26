using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoinRoomDlgControl : MonoBehaviour {

	// Use this for initialization
    public NumberShowControl[] showControls;

    private int currentIndex = 0;
    private int[] pushNumberArray = new int[6];

    private HallControl hallControl = null;
	void Start () {
        Clear();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetHallControl(HallControl control)
    {
        hallControl = control;
    }


    public void ResetBtnClick()
    {
        Clear();
    }

    public void DeleteBtnClick()
    {
        if (currentIndex == 0)
        {
            return;
        }

        currentIndex--;
        ShowNumber();
    }

    public void ExitBtnClick()
    {
        hallControl.JoinRoomDlg_ExitClick();
    }

    public void NumberBtnClick(int number)
    {
        pushNumberArray[currentIndex++] = number;
        ShowNumber();

        if (currentIndex == 6)
        {
            //数字按满了触发进入房间的请求
            string strRoomId = GetRoomId();
            int roomId = int.Parse(strRoomId);
            hallControl.JoinRoomDlg_JoinRoomClick(roomId);
        }
    }

    private void ShowNumber()
    {
        for (int i=0; i<currentIndex; i++)
        {
            showControls[i].SetNumber(pushNumberArray[i]);
        }

        for (int i = currentIndex; i < 6; i++)
        {
            showControls[i].Clear();
        }
    }

    private void Clear()
    {
        foreach (var item in showControls)
        {
            item.Clear();
        }

        currentIndex = 0;
    }

    private string GetRoomId()
    {
        string value = "";
        for (int i=0; i<currentIndex; i++)
        {
            value = value + string.Format("{0}", pushNumberArray[i]);
        }

        return value;
    }
}
