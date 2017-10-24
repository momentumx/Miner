using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableScript : MonoBehaviour {
	private void OnDisable()
	{
		Destroy(transform.GetChild(0).gameObject);
	}
}
