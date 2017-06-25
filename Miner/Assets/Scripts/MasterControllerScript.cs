using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterControllerScript : MonoBehaviour {

	public enum SAVEKEYINT {
		Level,

		Total
	}
	public enum SAVEKEYFLT {
		MusicVolume, SfxVolume, Speed,

		Total
	}

	public enum MUSIC_CLIP {
		None,
		Start,
		Gameplay,
		Boss,
		Victory,
		Lose,
		Stop
	}

	public enum TILE_VALUES {
		Dirt, Stone, Oil, Artifact, Map, Coal, Copper, Iron, Opal, Silver, Gold, Ruby, Emerald, Azurite, Amythyst, Onyx, Pearl, Sapphire, Topaz, Titanium, Platinum, Kryptonium,
		Plutonium, Treasure,

		Total
	}
	public enum TILE_STATES {
		Hidden, Visited, Dug, Placed
	}
	public enum TILE_GEMTYPES {
		Single, Mineral, Rhombus, Dagger, Diamond, Heart, Hexagon, Leaf, Triangle
	}

	static public MUSIC_CLIP currClip = MUSIC_CLIP.Start;
	static public AudioClip[] music_clips, audio_attacks, audio_deaths;
	static public AudioClip armor;
	public static float musicVol, sfxVol;
	public static AudioSource musicPlayer, sfxPlayer, transition;

	void Start () {
		DontDestroyOnLoad ( gameObject );
		musicPlayer = GetComponents<AudioSource> () [ 0 ];
		sfxPlayer = GetComponents<AudioSource> () [ 1 ];
		transition = GetComponents<AudioSource> () [ 2 ];
		musicPlayer.ignoreListenerVolume = true;
		//UnityEngine.SceneManagement.SceneManager.LoadScene ( "MainMenu" );
		if ( PlayerPrefs.HasKey ( SAVEKEYINT.Level.ToString () ) ) {
			//load ints

			//load floats
			//musicVol = PlayerPrefs.GetFloat ( SAVEKEYFLT.MusicVolume.ToString () );
			//sfxVol = PlayerPrefs.GetFloat ( SAVEKEYFLT.SfxVolume.ToString () );
			LoadStage ();
		} else {
			SaveStage ();
		}


		uint width = 240u, height = 30u;
		Vector2 gridPos;
		GameObject cloneTile = Resources.Load<GameObject>("Tile");
		uint x = uint.MaxValue; while ( ++x != width ) {
			uint y = uint.MaxValue;
			while ( ++y != height ) {
				gridPos.x = 2u * x;
				gridPos.y = -2u * y;
				Tile newTile = Instantiate ( cloneTile, gridPos, Quaternion.AngleAxis ( 90 * Random.Range ( 0, 4 ), Vector3.forward )).GetComponent<Tile>();
				// 20 oil 50 stone 10 map 1 artifact 330 artifact
				int chance = Random.Range ( 0, 500 );
				if ( chance < 330 )
					newTile.value = ( byte )TILE_VALUES.Dirt;
				else if ( chance < 337 )
					newTile.value = ( byte )TILE_VALUES.Stone;
				else if ( chance < 340 )
					newTile.value = ( byte )TILE_VALUES.Oil;
				else if ( chance < 345 )
					newTile.value = ( byte )TILE_VALUES.Map;
				else if ( chance == 345 )
					newTile.value = ( byte )TILE_VALUES.Artifact;
				else
					newTile.value = ( byte )Map ( Mathf.Min ( 1000f, (y*.5f + 2f) * Mathf.Log((chance - 345),1.3f ) ), 0f, 1000f, ( float )TILE_VALUES.Coal, ( float )( TILE_VALUES.Total - 1 ) );

				switch ( ( TILE_VALUES )newTile.value ) {
					case TILE_VALUES.Coal:
					case TILE_VALUES.Copper:
					case TILE_VALUES.Iron:
					case TILE_VALUES.Silver:
					case TILE_VALUES.Gold:
						newTile.gemType = ( byte )TILE_GEMTYPES.Mineral;
						break;

					case TILE_VALUES.Ruby:
					case TILE_VALUES.Topaz:
						newTile.gemType = ( byte )TILE_GEMTYPES.Dagger;
						break;
					case TILE_VALUES.Opal:
					case TILE_VALUES.Emerald:
						newTile.gemType = ( byte )TILE_GEMTYPES.Rhombus;
						break;
					case TILE_VALUES.Azurite:
						newTile.gemType = ( byte )TILE_GEMTYPES.Heart;
						break;
					case TILE_VALUES.Amythyst:
						newTile.gemType = ( byte )TILE_GEMTYPES.Leaf;
						break;
					case TILE_VALUES.Onyx:
						newTile.gemType = ( byte )TILE_GEMTYPES.Triangle;
						break;
					case TILE_VALUES.Pearl:
						newTile.gemType = ( byte )TILE_GEMTYPES.Hexagon;
						break;
					case TILE_VALUES.Sapphire:
						newTile.gemType = ( byte )TILE_GEMTYPES.Diamond;
						break;

					default:
						newTile.gemType = ( byte )TILE_GEMTYPES.Single;
						break;
				}

			}
		}

	}

	static public float Map ( float value, float minlow, float minHigh, float maxLow, float maxHigh ) {

		return ( maxLow + ( maxHigh - maxLow ) * ( value - minlow ) / ( minHigh - minlow ) );
	}

	static public float Map ( int value, int minlow, int minHigh, int maxLow, int maxHigh ) {
		int depthval = (int)(maxLow + ( maxHigh - maxLow ) * ( value - minlow ) / ( minHigh - minlow ));
		return depthval;
	}

	static public void SaveStage () {

	}

	static public void LoadStage () {

	}

	public void ResetKeys () {
		uint i=uint.MaxValue;
		while ( ++i != ( uint )SAVEKEYINT.Total )
			PlayerPrefs.SetInt ( System.Enum.GetName ( typeof ( SAVEKEYINT ), i ), 0 );
		i = uint.MaxValue;
		while ( ++i != ( uint )SAVEKEYFLT.Total )
			PlayerPrefs.SetFloat ( System.Enum.GetName ( typeof ( SAVEKEYFLT ), i ), 0f );
	}

	static public void SetSaveKey ( SAVEKEYINT _saveKey, int _val ) {
		PlayerPrefs.SetInt ( _saveKey.ToString (), _val );
	}

	static public void SetSaveKey ( SAVEKEYFLT _saveKey, float _val ) {
		PlayerPrefs.SetFloat ( _saveKey.ToString (), _val );
	}

	static public int SaveKeyInc ( SAVEKEYINT _saveKey ) {
		int val = PlayerPrefs.GetInt(_saveKey.ToString())+1;
		PlayerPrefs.SetInt ( _saveKey.ToString (), val );
		return val;
	}

	static public void SwitchMusic ( MUSIC_CLIP _newclip ) {
		if ( _newclip == currClip ) {
			return;
		}
		musicPlayer.Stop ();
		currClip = _newclip;
		musicPlayer.PlayOneShot ( music_clips [ ( uint )_newclip ] );
	}
}
