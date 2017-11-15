using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPopScript : MonoBehaviour {

	private void OnEnable()
	{

	}

	public void Close()
	{

	}

	IEnumerator Open()
	{
		float timeToShrink = Time.time + .3f;
		while (Time.time > timeToShrink)
		{
			transform.localScale *= 1.25f;
			yield return null;
		}
		yield break;
	}

	IEnumerator Closing()
	{
		float timeToShrink = Time.time + .3f;
		while (Time.time > timeToShrink)
		{
			transform.localScale *= .8f;
			yield return null;
		}
		yield break;
	}
}
