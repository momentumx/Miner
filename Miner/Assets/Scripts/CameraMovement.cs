using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour {

	Transform target;

	void Start () {
		target = GameObject.Find ( "Player" ).transform;
	}

	void FixedUpdate () {
		transform.position += ( Vector3 )(( ( Vector2 )target.position - ( Vector2 )transform.position ) * .03f);
		if ( transform.position.x > Screen.width )
			transform.position = new Vector2 ( Screen.width * .5f, transform.position.y );
		else if ( transform.position.x < 1f )
			transform.position = new Vector2 ( 1f, transform.position.y );
	}

	void Update () {
		Camera.main.orthographicSize -= Input.GetAxis ( "WheelZoom" ) * .03f;
		Mathf.Clamp ( Camera.main.orthographicSize, 4f, 6f );
		
	}

	static public void Win () {
		
	}

	static public void Lose () {

	}
}
