using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
	enum STATE
	{
		Normal,
		Digging,
		Celebrating,
		Dying,
		Breaking
	}
	[SerializeField]
	STATE state;
	enum DIRECTION
	{
		Up,
		Right,
		Down,
		Left,
		Still
	}
	[SerializeField]
	DIRECTION digDirection;
	UnityEngine.UI.RawImage contrB, contrJ;
	UnityEngine.UI.Text txt;
	Rigidbody2D rigid;
	Animator anim;
	[SerializeField]
	float maxSpeed = .4f, backImageRadius = .185f;
	[SerializeField]
	Transform knob;
	[SerializeField]
	Tile defender;
	[SerializeField]
	LayerMask tiles;
	int animSpeedHash;
	ParticleSystem dirtParticle, dropItem;
	string notice;
	bool gotItems;
	public static sbyte drillStrength = 1;
	public static uint bagSize;

	// Use this for initialization
	void Start()
	{
		txt = GameObject.Find("AmountTxt").GetComponent<UnityEngine.UI.Text>();
		dirtParticle = GameObject.Find("DirtExplosion").GetComponent<ParticleSystem>();
		dropItem = GameObject.Find("DropItem").GetComponent<ParticleSystem>();
		animSpeedHash = Animator.StringToHash("Speed");
		rigid = GetComponent<Rigidbody2D>();
		anim = GetComponent<Animator>();
		contrB = GameObject.Find("controller").GetComponent<UnityEngine.UI.RawImage>();
		contrJ = GameObject.Find("knob").GetComponent<UnityEngine.UI.RawImage>();
	}


	// Update is called once per frame
	void Update()
	{
		if (gotItems)
		{
			txt.text = notice + dropItem.particleCount;
			gotItems = false;
		}
#if UNITY_EDITOR

		if (Input.GetMouseButton(0))
		{
			if (Input.GetMouseButtonDown(0))
			{
				Color tempB = contrB.color;
				Color tempJ = contrJ.color;
				tempB.a = .4f;
				tempJ.a = tempB.a;
				contrB.color = tempB;
				contrJ.color = tempJ;
				contrB.transform.position = Input.mousePosition;
			}
			else
			{
				Vector2 speed = Vector2.ClampMagnitude(Input.mousePosition - contrB.transform.position, 100f);
				contrJ.transform.localPosition = speed * backImageRadius * .01f; // remove when we have actual image
				Vector3 scale = transform.localScale;
				scale.x = Mathf.Sign(speed.x) * Mathf.Abs(scale.x);
				transform.localScale = scale;
				switch (state)
				{
					case STATE.Normal:
						digDirection = StrongPush(speed * .01f);
						if (CheckDig(digDirection))
						{
							state = STATE.Digging;
							rigid.velocity = Vector2.zero;
							//defender.gameObject.isStatic = false;
							anim.SetTrigger("Attack");
							return;
						}
						if (rigid.gravityScale == 0f)
						{
							rigid.velocity = speed * maxSpeed * .01f;
							anim.SetFloat(animSpeedHash, Mathf.Abs(rigid.velocity.y / maxSpeed));

						}
						else
						{
							rigid.velocity = speed * maxSpeed * .01f + Physics2D.gravity;
							anim.SetFloat(animSpeedHash, Mathf.Abs(rigid.velocity.x / (maxSpeed)));
						}
						break;
					case STATE.Digging:
						anim.SetFloat(animSpeedHash, speed.y * .01f);
						if (StrongPush(speed * .01f) != digDirection)
						{
							state = STATE.Normal;
							anim.SetTrigger("Done");
						}
						break;
					case STATE.Celebrating:
						break;
					case STATE.Dying:
						break;
					case STATE.Breaking:
						break;
					default:
						break;
				}
			}
		}
		else if (Input.GetMouseButtonUp(0))
		{
			if (state == STATE.Digging)
			{
				state = STATE.Normal;
				anim.SetTrigger("Done");
			}
			anim.SetFloat(animSpeedHash, 0f);
		}
		else
		{
			Vector2 speed = Vector2.zero;
			if (rigid.velocity.y < 0f && rigid.gravityScale == 1f)
				speed.y = rigid.velocity.y;
			rigid.velocity = speed;
			if (contrB.color.a != 0)
			{
				Color tempB = contrB.color;
				Color tempJ = contrJ.color;
				tempB.a -= .06f;
				tempJ.a = tempB.a;
				contrB.color = tempB;
				contrJ.color = tempJ;
			}
		}

#else

		if ( Input.touchCount != 0 ) {
			if ( Input.GetTouch ( 0 ).phase == TouchPhase.Began ) {
				Color tempB = contrB.color;
				Color tempJ = contrJ.color;
				tempB.a = .4f;
				tempJ.a = tempB.a;
				contrB.color = tempB;
				contrJ.color = tempJ;
				contrB.transform.position = Input.mousePosition;
			} else if ( Input.GetTouch ( 0 ).phase == TouchPhase.Moved || Input.GetTouch ( 0 ).phase == TouchPhase.Stationary ) {
				Vector2 speed = Vector2.ClampMagnitude ( Input.mousePosition - contrB.transform.position, 100f );
				contrJ.transform.localPosition = speed * backImageRadius * .01f; // remove when we have actual image
				Vector3 scale = transform.localScale;
				scale.x = Mathf.Sign ( speed.x ) * Mathf.Abs ( scale.x );
				transform.localScale = scale;
				switch ( state ) {
					case STATE.Normal:
						digDirection = StrongPush ( speed * .01f );
						if ( CheckDig ( digDirection ) ) {
							state = STATE.Digging;
							rigid.velocity = Vector2.zero;
							defender.gameObject.isStatic = false;
							anim.SetTrigger ( "Attack" );
							return;
						}
						if ( rigid.gravityScale == 0f ) {
							rigid.velocity = speed * maxSpeed * .01f;
						} else {
							rigid.velocity = speed * maxSpeed * .01f + Physics2D.gravity;
						}
						anim.SetFloat ( animSpeedHash, Mathf.Abs ( rigid.velocity.x / ( maxSpeed) ) );
						break;
					case STATE.Digging:
						anim.SetFloat ( animSpeedHash, speed.y * .01f );
						if ( StrongPush ( speed * .01f ) != digDirection ) {
							state = STATE.Normal;
							anim.SetTrigger ( "Done" );
						}
						break;
					case STATE.Celebrating:
						break;
					case STATE.Dying:
						break;
					case STATE.Breaking:
						break;
					default:
						break;
				}
			} else {
				if ( state == STATE.Digging ) {
					state = STATE.Normal;
					anim.SetTrigger ( "Done" );
				}
				anim.SetFloat ( animSpeedHash, 0f );
			}
		} else {
			Vector2 speed = Vector2.zero;
			if ( rigid.velocity.y < 0f && rigid.gravityScale == 1f )
				speed.y = rigid.velocity.y;
			rigid.velocity = speed;
			if ( contrB.color.a != 0 ) {
				Color tempB = contrB.color;
				Color tempJ = contrJ.color;
				tempB.a -= .06f;
				tempJ.a = tempB.a;
				contrB.color = tempB;
				contrJ.color = tempJ;
			}
		}
#endif
	}

	void OnTriggerEnter2D(Collider2D _other)
	{
		anim.SetBool("Climb", true);
		rigid.gravityScale = 0f;
	}
	void OnTriggerExit2D(Collider2D _other)
	{
		rigid.gravityScale = 1f;
		anim.SetBool("Climb", false);
	}

	public void AttackTile()
	{
		defender.hp -= drillStrength;
		Transform defChild = defender.transform.GetChild(0), particleT = dirtParticle.transform, particlePosition = transform.GetChild(0);
		particleT.position = particlePosition.position;
		dirtParticle.Play();
		if (defender.hp < 1)
		{
			if (defender.value > (int)MasterControllerScript.TILE_VALUES.Treasure)//if we are goign to hace
			{
				txt.transform.position = Camera.main.WorldToScreenPoint(defender.transform.position);
				txt.color = defChild.GetChild(0).GetComponent<SpriteRenderer>().color;
				dropItem.transform.position = defender.transform.position;
				dropItem.Play();
				var temp = dropItem.textureSheetAnimation;
				temp.rowIndex = defender.value - (int)MasterControllerScript.TILE_VALUES.Coal;
				MasterControllerScript.mineralAmounts[temp.rowIndex] += dropItem.particleCount;
				notice = ((MasterControllerScript.TILE_VALUES)defender.value).ToString() + " +";
				anim.SetTrigger("Celebrate");
				state = STATE.Celebrating;
			}
			else
			{
				anim.SetTrigger("Done");
				state = STATE.Normal;
			}
			// check tile above and see if it is placed, and then check lowest undug tile and place it there
			defender.state = (byte)MasterControllerScript.TILE_STATES.Dug;
			//Destroy ( defender.gameObject );
			Destroy(defender.GetComponent<Collider>());
			Destroy(defChild.gameObject);
			return;
		}
		if (defender.hp == 1)
		{
			Transform crack = defChild.GetChild(1);
			crack.gameObject.SetActive(true);
			crack.rotation = Quaternion.AngleAxis(AngleOfHit_90s((Vector2)defender.transform.position - (Vector2)transform.position), Vector3.forward);
			crack.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Crack" + defender.hp);
			string werwer = crack.GetComponent<SpriteRenderer>().sprite.name;
			string tempstr = Regex.Match(werwer, "\\d+").Value;
			int termer = int.Parse(tempstr);

		}
		else if (defender.hp == 4)
		{
			
		}
		else
		{

		}
		StartCoroutine(Shake(defChild, .5f, .3f));
	}

	public void FinishCelebrating()
	{
		state = STATE.Normal;
	}

	float AngleOfHit_90s(Vector2 _dir)
	{
		if (Mathf.Abs(_dir.x) > Mathf.Abs(_dir.y))
			return _dir.x > 0f ? 0f : 180f;
		else
			return _dir.y > 0f ? 90f : 270f;
	}

	//Vector2 AngleOfHit_Vector ( Vector2 _dir ) {
	//	if ( Mathf.Abs ( _dir.x ) > Mathf.Abs ( _dir.y ) ) {
	//		if ( _dir.x > 0f )
	//			return Vector2.right;
	//		else
	//			return Vector2.left;
	//	} else {
	//		if ( _dir.y > 0f )
	//			return Vector2.up;
	//		else
	//			return Vector2.down;
	//	}
	//}

	DIRECTION StrongPush(Vector2 _dir)
	{
		if (_dir.x > .707f)
			return DIRECTION.Right;
		if (_dir.x < -.707f)
			return DIRECTION.Left;
		if (_dir.y < -.707f)
			return DIRECTION.Down;
		if (_dir.y > .707f)
			return DIRECTION.Up;
		return DIRECTION.Still;
	}

	public bool GetTile(float _x, float _y)
	{
		Vector2 hitPoint = transform.position;
		Collider2D hitCollider;
		hitPoint.x += _x;
		hitPoint.y += _y;
		hitCollider = Physics2D.OverlapCircle(hitPoint, .01f, tiles);
		if (hitCollider)
		{
			Tile temp = hitCollider.GetComponent<Tile>();
			if (temp.value != (byte)MasterControllerScript.TILE_VALUES.Stone)
			{
				defender = temp;
				return true;
			}
		}
		defender = null;
		return false;
	}

	bool CheckDig(DIRECTION _dir)
	{
		if (_dir == DIRECTION.Still)
		{
			defender = null;
			return false;
		}
		switch (_dir)
		{
			case DIRECTION.Up:
				return GetTile(0f, 2.3f);
			case DIRECTION.Right:
				return GetTile(.3f, .43f);
			case DIRECTION.Down:
				return GetTile(0f, -.3f);
			case DIRECTION.Left:
				return GetTile(-.3f, .43f);
			default:
				return false;
		}
	}

	IEnumerator Shake(Transform _trans, float _duration, float _magnitude)
	{
		float elapsed = 0f;

		Vector2 offSet;

		while (elapsed < _duration)
		{
			if (_trans == null)
				yield break;

			float damper = (1f - elapsed / _duration) * _magnitude;
			offSet.x = Random.Range(-1f, 1f) * damper;
			offSet.y = Random.Range(-1f, 1f) * damper;

			_trans.localPosition = offSet;
			_trans.localRotation = Quaternion.AngleAxis(Random.Range(-45f, 45f) * damper, _trans.forward);

			elapsed += Time.deltaTime;
			yield return null;
		}
		if (_trans)
			_trans.localPosition = Vector2.zero;
		yield return null;
	}
}