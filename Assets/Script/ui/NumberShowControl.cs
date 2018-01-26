using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumberShowControl : MonoBehaviour {

	// Use this for initialization

	void Start () {
        Log.Logic("NumberShowControl Start");
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetNumber(int number)
    {
        UISprite sprite = this.GetComponent<UISprite>();
        string spriteName = string.Format("{0}@2x", number);
        sprite.spriteName = spriteName;
        gameObject.SetActive(true);
    }

    public void Clear()
    {
        gameObject.SetActive(false);
    }

}
