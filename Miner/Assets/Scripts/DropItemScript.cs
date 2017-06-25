using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropItemScript : MonoBehaviour {
	ParticleSystem jewels;
	// Use this for initialization
	void Start () {
		jewels = transform.GetChild ( 0 ).GetComponent<ParticleSystem> ();
		var temp = jewels.textureSheetAnimation;
		temp.rowIndex = 2;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
