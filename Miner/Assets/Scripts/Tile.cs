using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {

	public byte value, state, gemType;
	public sbyte hp;

	[SerializeField]
	Color jewelColor;

	SpriteRenderer jewel;

	public void Test () {

	}

	void Update () {
		if(jewel)
		jewel.color = jewelColor;
	}

	void OnTriggerEnter2D ( Collider2D _other ) {
		if ( state == ( byte )MasterControllerScript.TILE_STATES.Hidden ) {
			GameObject parent = transform.GetChild ( 0 ).gameObject;
			parent.SetActive ( true );
			state = ( byte )MasterControllerScript.TILE_STATES.Visited;
			SpriteRenderer dirt= parent.GetComponent<SpriteRenderer> ();
			dirt.sprite = Resources.Load<Sprite> ( "Minerals/Dirt/Dirt"+ Random.Range ( 0, 4 ) );
			dirt.flipX = Random.Range ( 0, 2 ) == 0;
			if ( value != ( byte )MasterControllerScript.TILE_VALUES.Dirt ) {
				Transform temp = parent.transform.GetChild(0);
				temp.gameObject.SetActive ( true );
				jewel = temp.GetComponent<SpriteRenderer> ();
				int maxRandomSprites = Random.Range(0,2);
				if ( maxRandomSprites == 0 )
					jewel.flipX = true;
				// get the correct random sprite
				switch ( ( MasterControllerScript.TILE_GEMTYPES )gemType ) {
					case MasterControllerScript.TILE_GEMTYPES.Single:
						temp.transform.rotation = Quaternion.identity;
						jewel.sprite = Resources.LoadAll<Sprite> ( "Minerals/" + ( ( MasterControllerScript.TILE_VALUES )value ).ToString () ) [ maxRandomSprites ];
						break;
					case MasterControllerScript.TILE_GEMTYPES.Mineral:                   // this will probably change
						temp.transform.rotation = Quaternion.identity;
						jewel.sprite = Resources.LoadAll<Sprite> ( "Minerals/Mineral" ) [ Random.Range ( 0, 2 ) ];
						jewel.flipY = Random.Range ( 0, 2 ) == 0;
						break;
					default:
						temp.rotation = Quaternion.AngleAxis ( 90 * Random.Range ( 0, 4 ), Vector3.forward );
						jewel.sprite = Resources.LoadAll<Sprite> ( "Minerals/" + ( ( MasterControllerScript.TILE_GEMTYPES )gemType ).ToString () ) [ maxRandomSprites ];
						break;
				}

				//set the correct color
				switch ( ( MasterControllerScript.TILE_VALUES )value ) {
					case MasterControllerScript.TILE_VALUES.Coal:
						jewel.color = new Color ( .2f, .2f, .2f, 1f );
						break;
					case MasterControllerScript.TILE_VALUES.Copper:
						jewel.color = new Color ( 1f, .48f, .3f, 1f );
						break;
					case MasterControllerScript.TILE_VALUES.Iron:
						jewel.color = new Color ( .43f, .43f, .43f, 1f );
						break;
					case MasterControllerScript.TILE_VALUES.Opal:
						jewel.color = Color.cyan;
						break;
					case MasterControllerScript.TILE_VALUES.Gold:
						jewel.color = new Color ( 1f, 1f, 0f, 1f );
						break;
					case MasterControllerScript.TILE_VALUES.Ruby:
						jewel.color = Color.red;
						break;
					case MasterControllerScript.TILE_VALUES.Emerald:
						jewel.color = Color.green;
						break;
					case MasterControllerScript.TILE_VALUES.Azurite:
						jewel.color = Color.blue;
						break;
					case MasterControllerScript.TILE_VALUES.Amythyst:
						jewel.color = Color.magenta;
						break;
					case MasterControllerScript.TILE_VALUES.Onyx:
						jewel.color = new Color ( .15f, .15f, .15f, 1f );
						break;
					case MasterControllerScript.TILE_VALUES.Pearl:
						jewel.color = new Color ( .85f, .85f, .85f, 1f );
						break;
					case MasterControllerScript.TILE_VALUES.Sapphire:
						jewel.color = Color.red;
						break;
					case MasterControllerScript.TILE_VALUES.Topaz:
						jewel.color = Color.yellow;
						break;
					default:
						break;
				}
				jewelColor = jewel.color;
			}
		}
	}
}
