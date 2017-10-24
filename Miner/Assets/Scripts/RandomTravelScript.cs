using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomTravelScript : MonoBehaviour
{

	public Vector2 direction, aim, velocity;
	float timer, acc;

	// Use this for initialization
	public void Start()
	{
		velocity = new Vector2(Random.Range(0, 1f), Random.Range(0, -1f)).normalized * .2f;
		timer = Time.time + .1f;
	}

	public void FixedUpdate()
	{
		if (acc > .01f)
		{
			acc += .01f;
			if (acc > .9f)
				acc = .9f;
			direction = (aim - (Vector2)transform.position).normalized * .3f;
		}
		else
		{
			if (Time.time > timer)
			{
				direction = new Vector2(Random.Range(-1f, 1f), Random.Range(0, 1f)).normalized * .2f;
				timer = Time.time + .1f;
				acc += .001f;
			}

		}
		velocity += (direction - velocity) * (.1f + acc);

		transform.position += (Vector3)velocity;

	}
}
