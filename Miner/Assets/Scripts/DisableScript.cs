using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableScript : MonoBehaviour
{
	private void OnDisable()
	{
		if (transform.childCount != 0)
			Destroy(transform.GetChild(0).gameObject);
	}
}
