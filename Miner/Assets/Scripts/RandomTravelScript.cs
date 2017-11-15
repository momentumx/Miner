using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomTravelScript : MonoBehaviour
{
	[HideInInspector]
	public Transform aim;
	[HideInInspector]
	public Vector2 direction, offSet, velocity;
	[HideInInspector]
	public uint timer;
	[SerializeField]
	float moveSpeed, lirpSpeed, speedInc, lirpInc;

	// Use this for initialization
	public void Start()
	{
		InvokeRepeating("SetRandomPos", 0f, .04f);
		direction = Camera.main.ViewportToWorldPoint(new Vector3(Random.Range(.22f, .78f), Random.Range(.1f, .78f)));
		velocity = (direction - (Vector2)transform.position).normalized * .3f;
	}

	public void FixedUpdate()
	{
		if (aim)
		{
			if (timer != 0u)
			{
				timer -= 1u;
				if (timer == 0u)
				{
					CancelInvoke("SetRandomPos");
					direction = (Vector2)aim.position + offSet;
				}
				velocity = Vector2.Lerp(velocity, (direction - (Vector2)transform.position).normalized * moveSpeed, lirpSpeed);
				transform.position += (Vector3)(velocity.normalized * moveSpeed);
			}
			else
			{
				if (Vector2.SqrMagnitude(direction - (Vector2)transform.position) < moveSpeed)
				{
					var rend = GetComponent<ParticleSystem>().main;
					transform.position = aim.position + (Vector3)offSet;
					Color blend = rend.startColor.color;
					blend.a -= .01f;
					if (blend.a < .01f)
					{
						Destroy(gameObject);
					}
					rend.startColor = blend;
				}
				else
				{
					velocity = Vector2.Lerp(velocity, (direction - (Vector2)transform.position).normalized * moveSpeed, lirpSpeed);
					transform.position += (Vector3)(velocity.normalized * moveSpeed);
				}
			}
		}
		else
		{
			Destroy(gameObject);
		}
	}

	void SetRandomPos()
	{
		direction = Camera.main.ViewportToWorldPoint(new Vector3(Random.Range(.22f, .78f), Random.Range(.1f, .78f)));
		moveSpeed += speedInc;
		lirpSpeed += lirpInc;
	}
}
