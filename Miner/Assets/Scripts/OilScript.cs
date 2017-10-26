using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OilScript : MonoBehaviour {

	private void OnTriggerEnter2D(Collider2D collision)
	{
		GetComponent<Animator>().SetBool("Transition", true);
	}

	public void ChangeTile()
	{
		Transform invade = Physics2D.Raycast((Vector2)transform.position + Vector2.down, Vector2.zero).transform;
		invade.GetComponent<Tile>().value = (byte)MCScript.TILE_VALUES.Oil;
		invade.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite = Resources.LoadAll<Sprite>("/Minerals/Oil")[Random.Range(0, 2)];
	}

	public void DeleteMe()
	{
		Destroy(gameObject);
	}
}
