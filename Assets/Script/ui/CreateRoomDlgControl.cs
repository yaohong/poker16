using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateRoomDlgControl : MonoBehaviour {

    private HallControl hallControl;
	// Use this for initialization

    public void SetHallControl(HallControl hc)
    {
        hallControl = hc;
    }

    //玩家AA
    public void AABtnClick()
    {
        hallControl.CbCreateRoom("aaRoom", 200, true, true, true, true, true, true);
        GameObject.Destroy(this.gameObject);
    }

    //包场
    public void SelfBtnClick()
    {
        hallControl.CbCreateRoom("selfRoom", 200, false, true, true, true, true, true);
        GameObject.Destroy(this.gameObject);
    }

    public void ExitClick()
    {
        GameObject.Destroy(this.gameObject);
    }


}
