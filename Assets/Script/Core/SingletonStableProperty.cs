using UnityEngine;

public class SingletonStableProperty : MonoBehaviour {
    private static GameObject ins = null;

    void Awake() {
        if (ins == null) {
            DontDestroyOnLoad(gameObject);
            ins = gameObject;
        } else {
            Debug.Log("<color=red>SingletonStableProperty Recreate, Destroy it! name=" + name + "</color>");
            DestroyImmediate(gameObject);
        }
    }
}
