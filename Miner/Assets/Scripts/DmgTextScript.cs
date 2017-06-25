using UnityEngine;

public class DmgTextScript : MonoBehaviour {
	void Start () {
		transform.SetParent ( GameObject.Find ( "Canvas" ).transform );
		Destroy ( gameObject, 1f );
	}
	
	void FixedUpdate () {
		Vector2 upDir = transform.position;
		++upDir.y;
		transform.position = upDir;
	}
}
