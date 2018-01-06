using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLoading : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Invoke ("SwitchLogin", 2);
	}
	
	// Update is called once per frame
	void SwitchLogin () {
		UILib.SwitchProcedurePanel ("LOGIN");
	}
}
