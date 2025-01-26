using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPopScript : MonoBehaviour {
	[Range(.001f, .95f)]
	[SerializeField]
	float startScale = .5f, elasticity = .9f;
	[SerializeField]
	float maxLength = .3f, speedDivisor;

	float naturalBounce = .9f;

	private void OnEnable()
	{
		transform.localScale *= startScale;
		StartCoroutine(Open());
	}

	public void Close()
	{

	}

	IEnumerator Open()
	{
		yield return new WaitForEndOfFrame();
		float timeToShrink = Time.time + maxLength;
		Vector3 scale = new Vector3(startScale, startScale, 1f);
		float speed = Mathf.Abs(startScale - 1f)*2 /maxLength/ speedDivisor;
		while (Time.time < timeToShrink)
		{
			float sign = Mathf.Sign(1 - scale.x);
			scale.x += sign*speed;
			scale.y += Mathf.Sign(1 - scale.y)*speed;
			speed -= elasticity*elasticity*sign;
			transform.localScale = scale;
			yield return new WaitForFixedUpdate();
		}
		transform.localScale = Vector3.one;
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
