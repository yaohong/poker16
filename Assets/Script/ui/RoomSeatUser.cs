using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomSeatUser : MonoBehaviour {

    public UILabel nickNameLable;
    public UISprite iconSprite;
	// Use this for initialization

    private string nickname;
    private string avatarUrl;
    private int seatNumber;
    
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetSeatNumber(int tmpSeatNumber)
    {
        seatNumber = tmpSeatNumber;
    }

    public void Sitdown(string tmpNickname, string tmpAvatarUrl)
    {
        nickname = tmpNickname;
        avatarUrl = "test_url";
        Refresh();
    }

    public void Standup()
    {
        nickname = "";
        avatarUrl = "";
        Refresh();
    }


    void Refresh()
    {
        nickNameLable.text = nickname;
        if (avatarUrl == "")
        {
            iconSprite.spriteName = "";
        } 
        else
        {
            iconSprite.spriteName = "？框@2x";
        }
    }

}
