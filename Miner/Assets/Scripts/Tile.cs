using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Tile : MonoBehaviour  {

	public byte value, state;
	public sbyte hp;
	SpriteRenderer jewel;

	// this all needs to be removed when we have final colors
	[SerializeField]
	Color jewelColor;
	void Update () {
		if(jewel)
		jewel.color = jewelColor;
	}

	public void OnTriggerEnter2D ( Collider2D _other ) {
		if ( state == ( byte )MCScript.TILE_STATES.Hidden ) {
			Initialized();
		}
	}

	public void Initialized()
	{
		GameObject parent = transform.GetChild(0).gameObject;
		parent.SetActive(true);
		state = (byte)MCScript.TILE_STATES.Visited;
		SpriteRenderer dirt = parent.GetComponent<SpriteRenderer>();
		dirt.sprite = Resources.Load<Sprite>("Minerals/Dirt/Dirt" + UnityEngine.Random.Range(0, 4));
		dirt.flipX = UnityEngine.Random.Range(0, 2) == 0;
		if (value != (byte)MCScript.TILE_VALUES.Dirt)
		{
			Transform temp = parent.transform.GetChild(0);
			temp.gameObject.SetActive(true);
			jewel = temp.GetComponent<SpriteRenderer>();
			int maxRandomSprites = UnityEngine.Random.Range(0, 2);
			if (maxRandomSprites == 0)
				jewel.flipX = true;
			// get the correct random sprite
			MCScript.TILE_GEMTYPES gemType;
			switch ((MCScript.TILE_VALUES)value)
			{
				case MCScript.TILE_VALUES.Stone:
					gemType = MCScript.TILE_GEMTYPES.Single;
					gameObject.layer = LayerMask.NameToLayer("Stone");
					break;
				case MCScript.TILE_VALUES.Coal:
				case MCScript.TILE_VALUES.Copper:
				case MCScript.TILE_VALUES.Nickel:
				case MCScript.TILE_VALUES.Zinc:
				case MCScript.TILE_VALUES.Tin:
				case MCScript.TILE_VALUES.Aluminum:
				case MCScript.TILE_VALUES.Chromium:
				case MCScript.TILE_VALUES.Iron:
				case MCScript.TILE_VALUES.Silver:
				case MCScript.TILE_VALUES.Gold:
					gemType = MCScript.TILE_GEMTYPES.Mineral;
					break;

				case MCScript.TILE_VALUES.Ruby:
				case MCScript.TILE_VALUES.Topaz:
					gemType = MCScript.TILE_GEMTYPES.Dagger;
					break;
				case MCScript.TILE_VALUES.Opal:
				case MCScript.TILE_VALUES.Emerald:
					gemType = MCScript.TILE_GEMTYPES.Rhombus;
					break;
				case MCScript.TILE_VALUES.Azurite:
					gemType = MCScript.TILE_GEMTYPES.Heart;
					break;
				case MCScript.TILE_VALUES.Amethyst:
					gemType = MCScript.TILE_GEMTYPES.Leaf;
					break;
				case MCScript.TILE_VALUES.Onyx:
					gemType = MCScript.TILE_GEMTYPES.Triangle;
					break;
				case MCScript.TILE_VALUES.Pearl:
					gemType = MCScript.TILE_GEMTYPES.Hexagon;
					break;
				case MCScript.TILE_VALUES.Sapphire:
					gemType = MCScript.TILE_GEMTYPES.Diamond;
					break;

				default:
					gemType = MCScript.TILE_GEMTYPES.Single;
					break;
			}
			switch (gemType)
			{
				case MCScript.TILE_GEMTYPES.Single:
					temp.transform.rotation = Quaternion.identity;
					jewel.sprite = Resources.LoadAll<Sprite>("Minerals/" + ((MCScript.TILE_VALUES)value).ToString())[maxRandomSprites];
					break;
				case MCScript.TILE_GEMTYPES.Mineral:                   // this will probably change
					temp.transform.rotation = Quaternion.identity;
					jewel.sprite = Resources.LoadAll<Sprite>("Minerals/Mineral")[UnityEngine.Random.Range(0, 2)];
					jewel.flipY = UnityEngine.Random.Range(0, 2) == 0;
					break;
				default:
					temp.rotation = Quaternion.AngleAxis(90 * UnityEngine.Random.Range(0, 4), Vector3.forward);
					jewel.sprite = Resources.LoadAll<Sprite>("Minerals/" + (gemType).ToString())[maxRandomSprites];
					break;
			}

			//set the correct color
			switch ((MCScript.TILE_VALUES)value)
			{
				case MCScript.TILE_VALUES.Coal:
					jewel.color = new Color(.2f, .2f, .2f, 1f);
					break;
				case MCScript.TILE_VALUES.Copper:
					jewel.color = new Color(1f, .48f, .3f, 1f);
					break;
				case MCScript.TILE_VALUES.Nickel:
					jewel.color = new Color(.55f, .55f, .55f, 1f);
					break;
				case MCScript.TILE_VALUES.Zinc:
					jewel.color = new Color(.65f, .65f, .65f, 1f);
					break;
				case MCScript.TILE_VALUES.Iron:
					jewel.color = new Color(.43f, .43f, .43f, 1f);
					break;
				case MCScript.TILE_VALUES.Tin:
					jewel.color = new Color(.75f, .75f, .75f, 1f);
					break;
				case MCScript.TILE_VALUES.Aluminum:
					jewel.color = new Color(.85f, .85f, .85f, 1f);
					break;
				case MCScript.TILE_VALUES.Opal:
					jewel.color = Color.cyan;
					break;
				case MCScript.TILE_VALUES.Silver:
					jewel.color = new Color(.95f, .95f, .95f, 1f);
					break;
				case MCScript.TILE_VALUES.Gold:
					jewel.color = new Color(1f, 1f, 0f, 1f);
					break;
				case MCScript.TILE_VALUES.Ruby:
					jewel.color = Color.red;
					break;
				case MCScript.TILE_VALUES.Emerald:
					jewel.color = Color.green;
					break;
				case MCScript.TILE_VALUES.Azurite:
					jewel.color = Color.blue;
					break;
				case MCScript.TILE_VALUES.Amethyst:
					jewel.color = Color.magenta;
					break;
				case MCScript.TILE_VALUES.Onyx:
					jewel.color = new Color(.15f, .15f, .15f, 1f);
					break;
				case MCScript.TILE_VALUES.Pearl:
					jewel.color = new Color(.85f, .85f, .85f, 1f);
					break;
				case MCScript.TILE_VALUES.Sapphire:
					jewel.color = Color.red;
					break;
				case MCScript.TILE_VALUES.Topaz:
					jewel.color = Color.yellow;
					break;
				default:
					break;
			}
			jewelColor = jewel.color;
		}
	}
}
