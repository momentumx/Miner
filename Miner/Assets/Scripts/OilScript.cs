using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OilScript : MonoBehaviour
{

	Transform hit;
	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.tag == "Player")
		{
			// TODO:
			// make the palyer turn black
		}
		else
		{
			hit = collision.transform;
			Destroy(gameObject);
			ChangeTile();
			// Todo
			//StartCoroutine(Fill());
		}
	}

	public void ChangeTile()
	{
		Transform invade = Physics2D.Raycast((Vector2)transform.position + Vector2.down, Vector2.zero).transform;
		invade.GetComponent<Tile>().value = (byte)MCScript.TILE_VALUES.Oil;
		invade.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite = Resources.LoadAll<Sprite>("Minerals/Oil")[Random.Range(0, 2)];
		invade.GetChild(0).gameObject.SetActive(true);
		invade.GetChild(0).GetChild(0).gameObject.SetActive(true);
	}

	private void FixedUpdate()
	{
		if (!hit)
		{
			transform.position += new Vector3(0f,-.01f);
			transform.GetChild(0).localScale += new Vector3(-.001f,.01f);
		}
	}

	IEnumerator Fill()
	{
		float dist = Vector2.Distance(transform.position, hit.position);
		while ( dist >.1f)
		{
			transform.position += Vector3.down * .01f;
			transform.localScale += Vector3.Lerp(transform.localScale, Vector3.one, .1f / dist);
			yield return null;
		}
		transform.GetChild(1).gameObject.SetActive(true);
		transform.position = hit.position;
		transform.localScale = Vector3.one;
		SpriteRenderer rendFade = transform.GetChild(1).GetComponent<SpriteRenderer>();
		SpriteRenderer rendOil = transform.GetChild(0).GetComponent<SpriteRenderer>();
		while (rendFade.color.g > .05f)
		{
			rendFade.color = Color.Lerp(rendFade.color, Color.black, .05f);
			rendOil.color = Color.Lerp(rendOil.color, Color.clear, .05f);
			yield return null;
		}
		ChangeTile();
		rendFade.color = Color.black;
		rendOil.color = Color.clear;

		Destroy(gameObject);
		yield break;
	}
}
