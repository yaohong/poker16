using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockedControl : MonoBehaviour {

	// Use this for initialization
    public UILabel stateLable;
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    public void SetState(string state)
    {
        stateLable.text = state;
    }


}
