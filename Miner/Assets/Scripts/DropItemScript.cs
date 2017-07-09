using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropItemScript : MonoBehaviour {

	void OnCollisionEnter2D(Collision2D _coll ) {
		transform.position = new Vector2 ( 0f, 1000f );

	}
}
