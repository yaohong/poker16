using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Common : MonoBehaviour {

    public GameObject blockedTempalte;
    static Common ins = null;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void Awake()
    {
        if (ins != null)
        {
            Debug.LogError("TcpManager Recreate!!");
        }

        ins = this;
    }

    public static Common Ins
    {
        get
        {
            return ins;
        }
    }
	// Use this for initialization
    public BlockedControl CreateBlocked(GameObject parent)
    {
        GameObject blockedObj = GameObject.Instantiate(blockedTempalte);
        UIPanel panel = blockedObj.GetComponent<UIPanel>();
        panel.depth = 10000;

        blockedObj.transform.parent = parent.transform;
        blockedObj.transform.localScale = Vector3.one;
        blockedObj.transform.localPosition = Vector3.one;

        return blockedObj.GetComponent<BlockedControl>();
    }
}
