using UnityEngine;

public class Health : MonoBehaviour {
	enum DMGINDICATOR {
		Flash,
		Number,
		Image,
		NumberAndImage,
		FlashAndImage,
		FlashAndNumber,
		All
	}
	[SerializeField]
	DMGINDICATOR dmgIndicator;
	enum HBARTYPE {
		ThreeD,
		TwoD
	}
	[SerializeField]
	HBARTYPE hBarType;
	[SerializeField]
	GameObject dmgInd;
	public uint armor, HP;
	[HideInInspector]
	public uint occupiedbits, avaiablespaces;
	protected uint flashTimer;
	protected Transform healthbar, camerapos;
	[HideInInspector]
	public float radius;
	[HideInInspector]
	public int currhp;
	Vector3 scale;
	Color color;
	public byte slots;

	virtual public void Start () {
		camerapos = Camera.main.transform;
		radius = transform.localScale.x * GetComponent<SphereCollider> ().radius;
		healthbar = transform.GetChild ( 0 ).GetChild ( 0 );
		color = healthbar.GetComponent<Renderer> ().material.color;
		scale = healthbar.localScale;
		currhp = ( int )HP;
		//this is confusing but it stores spots that can be attacked in an uint instead of holding a bunch of vectors
		if ( slots == 0 )
			slots = ( byte )Mathf.Min ( Mathf.Log ( radius * 5f, 2f ), 32f );
		int i = slots; while ( --i!=-1 )
			avaiablespaces |= ( 1u << i );

	}

	virtual public void FixedUpdate () {
		if ( hBarType == HBARTYPE.TwoD )
			healthbar.parent.LookAt ( camerapos );
		if ( flashTimer != 0u ) {
			--flashTimer;
			if ( flashTimer == 0u ) {
				healthbar.localScale = scale;
				healthbar.GetComponent<SpriteRenderer> ().color = color;
			}
		}
	}

	virtual public void TakeDamage ( int _dmg, Color _color, float _magnitude = 1 ) {
		if ( currhp != 0 ) {
			if ( _dmg > 0 ) {
				_dmg -= ( int )armor;
				if ( _dmg < 1 )
					_dmg = 1;
			}
			currhp -= ( int )_dmg;
			if ( currhp > HP ) {
				currhp = ( int )HP;
				healthbar.parent.gameObject.SetActive ( false );
			} else {
				if ( currhp < 0 ) {
					healthbar.parent.gameObject.SetActive ( false );
					currhp = 0;
				} else {
					healthbar.parent.gameObject.SetActive ( true );
					scale.x = currhp / HP;
					GameObject ind;
					switch ( dmgIndicator ) {
						case DMGINDICATOR.Flash:
							healthbar.localScale = new Vector2 ( scale.x, healthbar.localScale.y * 1.5f );
							healthbar.GetComponent<SpriteRenderer> ().color = _color;
							flashTimer = 4u;
							break;
						case DMGINDICATOR.Number:
							ind = Instantiate(dmgInd, Camera.main.WorldToScreenPoint(healthbar.parent.position), Quaternion.identity);
							ind.GetComponent<UnityEngine.UI.Text> ().text = ( _dmg < 1 ? '+' + ( -_dmg ).ToString () : _dmg.ToString () );
							ind.GetComponent<UnityEngine.UI.Text> ().color = _color;
							healthbar.localScale = scale;
							break;
						case DMGINDICATOR.Image:
							ind = Instantiate ( dmgInd, Camera.main.WorldToScreenPoint ( healthbar.parent.position ), Quaternion.identity );
							healthbar.localScale = scale;
							break;
						case DMGINDICATOR.FlashAndImage:
							healthbar.localScale = new Vector2 ( scale.x, healthbar.localScale.y * 1.5f );
							healthbar.GetComponent<SpriteRenderer> ().color = _color;
							flashTimer = 4u;
							ind = Instantiate ( dmgInd, Camera.main.WorldToScreenPoint ( healthbar.parent.position ), Quaternion.identity );
							break;
						case DMGINDICATOR.FlashAndNumber:
							healthbar.localScale = new Vector2 ( scale.x, healthbar.localScale.y * 1.5f );
							healthbar.GetComponent<SpriteRenderer> ().color = _color;
							flashTimer = 4u;
							ind = Instantiate ( dmgInd, Camera.main.WorldToScreenPoint ( healthbar.parent.position ), Quaternion.identity );
							ind.GetComponent<UnityEngine.UI.Text> ().text = ( _dmg < 1 ? '+' + ( -_dmg ).ToString () : _dmg.ToString () );
							ind.GetComponent<UnityEngine.UI.Text> ().color = _color;
							break;
						case DMGINDICATOR.NumberAndImage:
							ind = Instantiate( dmgInd, Camera.main.WorldToScreenPoint(healthbar.parent.position), Quaternion.identity);
							ind.GetComponent<UnityEngine.UI.Text> ().text = ( _dmg < 1 ? '+' + ( -_dmg ).ToString () : _dmg.ToString () );
							ind.GetComponent<UnityEngine.UI.Text> ().color = _color;
							if ( _dmg > 0 ) {
								ind.transform.GetChild ( 0 ).gameObject.SetActive ( true );
								MasterControllerScript.sfxPlayer.PlayOneShot ( MasterControllerScript.armor );
							}
							healthbar.localScale = scale;
							break;
						case DMGINDICATOR.All:
							healthbar.localScale = new Vector2 ( scale.x, healthbar.localScale.y * 1.5f );
							healthbar.GetComponent<SpriteRenderer> ().color = _color;
							ind = Instantiate ( dmgInd, Camera.main.WorldToScreenPoint ( healthbar.parent.position ), Quaternion.identity );
							ind.GetComponent<UnityEngine.UI.Text> ().text = ( _dmg < 1 ? '+' + ( -_dmg ).ToString () : _dmg.ToString () );
							ind.GetComponent<UnityEngine.UI.Text> ().color = _color;
							if ( _dmg > 0 ) {
								ind.transform.GetChild ( 0 ).gameObject.SetActive ( true );
								MasterControllerScript.sfxPlayer.PlayOneShot ( MasterControllerScript.armor );
							}
							break;
						default:
							break;
					}
				}
			}
		}
	}

	public void Restore () {

	}

}