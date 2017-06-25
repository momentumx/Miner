using UnityEngine;

public class destroytimer : MonoBehaviour {

    public float timer;
    public float collidertimer;
	// Use this for initialization
	void Start () {
        if(transform.parent!=null)
            Destroy(transform.parent.gameObject,timer);
        Destroy(gameObject, collidertimer);
	}
}
