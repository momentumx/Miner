using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParrallexScript : MonoBehaviour {
	//float width;
	//Transform target;
	//public float width;
	// Use this for initialization
	void Start () {
		//target = GameObject.Find ( "Player" ).transform;
		//width = GetComponent<SpriteRenderer>().sprite.bounds.size.x*transform.lossyScale.x*.5f;
	}

	private void LateUpdate()
	{
		transform.position = new Vector2(Camera.main.transform.position.x*.93f, 1f);
	}
}
