﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarknessScript : MonoBehaviour {

	SpriteRenderer image;
	Vector2 pos, scale, originScale;
	[SerializeField]
	Transform target;

	// Use this for initialization
	void Start () {
		image = GetComponent<SpriteRenderer>();
		InvokeRepeating("Randomize", .01f, .1f);
		originScale = transform.localScale;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (target.position.y < 30f)
		{
		Color alphaR = image.color;
		alphaR.a = Mathf.Clamp((transform.position.y +30f) / -80f,0f,1f);
		image.color = alphaR;
		float smoothness = .05f;
		transform.position = Vector2.Lerp(transform.position, pos,smoothness);
		transform.localScale = Vector2.Lerp(transform.localScale, scale,smoothness);
		}
		else
		{
			Color alphaR = image.color;
			alphaR.a = 0;
			image.color = alphaR;
		}
	}

	void Randomize()
	{
		pos = new Vector2(target.position.x + Random.Range(-1.5f,1.5f), target.position.y + Random.Range(-1.5f, 1.5f));
		uint light = CoppyScript.GetVision();
		scale = new Vector2(originScale.x + light + Random.Range(1f, 1.2f), originScale.y + light + Random.Range(1f, 1.2f));
	}
}
