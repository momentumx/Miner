using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
	public enum STATE { Normal, Digging, Celebrating, Dying, Breaking, Flying, WalkIn }
	public STATE state = STATE.Flying;
	int digDirection;
	UnityEngine.UI.RawImage contrB, contrJ;
	Rigidbody2D rigid;
	Animator anim;
	[SerializeField]
	GameObject flame;
	[SerializeField]
	float maxSpeed = .4f, backImageRadius = .185f;
	Tile defender;
	[SerializeField]
	LayerMask tiles;
	int animSpeedHash, animStateHash;
	ParticleSystem dirtParticle, dropItem;
	AudioSource audioS;
	string notice;
	bool gotItems;
	public static sbyte drillStrength = 1;
	static public uint maxBagSize;
	static public List<TypeAmount> bag = new List<TypeAmount>();
	static public ushort[] items = new ushort[MCScript.differentItemCount];

	// Use this for initialization
	void Start()
	{
		audioS = GetComponent<AudioSource>();
		var currTime = System.DateTime.Now;
		dirtParticle = GameObject.Find("DirtExplosion").GetComponent<ParticleSystem>();
		dropItem = GameObject.Find("DropItem").GetComponent<ParticleSystem>();
		animSpeedHash = Animator.StringToHash("Speed");
		animStateHash = Animator.StringToHash("State");
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
			MCScript.txt.text = notice + dropItem.particleCount;
			UnityEngine.UI.Text mineralText = GameObject.Find("MineralCountTxt").GetComponent<UnityEngine.UI.Text>();
			mineralText.text = (int.Parse(mineralText.text) + dropItem.particleCount).ToString();
			int match = bag.IndexOf((MCScript.TILE_VALUES)defender.value);
			if (match == -1)
			{
				match = bag.Count;
				bag.Add((MCScript.TILE_VALUES)defender.value);
			}
			bag[match] += (uint)dropItem.particleCount;
			gotItems = false;
		}
		if (CameraMovement.cameraMode == CameraMovement.CAMERA_MODE.UnderGround)
		{
			if (transform.position.y > 1.1f)
			{
				rigid.velocity = Vector2.zero;
				rigid.gravityScale = 0f;
				FindObjectOfType<CameraMovement>().GoVisit(3f);
				ChangeStates(STATE.WalkIn);
				ChangeControllerAlpha(0f);
				audioS.Stop();
				Vector3 newScale = transform.localScale;
				newScale.x = Mathf.Abs(transform.localScale.x);
				transform.localScale = newScale;
				MCScript.GoUp();
				return;
			}
#if UNITY_EDITOR
			if (Input.GetMouseButton(0))
			{
				if (Input.GetMouseButtonDown(0))
				{
					ChangeControllerAlpha(.4f);
					contrB.transform.position = Input.mousePosition;
				}
				else
				{
					Vector2 speed = Vector2.ClampMagnitude(Input.mousePosition - contrB.transform.position, 100f);
					speed *= .01f;// bring it back to a value of 1
					contrJ.transform.localPosition = speed * backImageRadius; // remove when we have actual image
					Vector3 scale = transform.localScale;
					scale.x = Mathf.Sign(speed.x) * Mathf.Abs(scale.x);
					transform.localScale = scale;
					switch (state)
					{
						case STATE.Flying:
						case STATE.Normal:

							digDirection = StrongPush(speed);
							if (CheckDig(digDirection))
							{
								ChangeStates(STATE.Digging);
								rigid.velocity = Vector2.zero;
								rigid.gravityScale = 0f;
								//defender.gameObject.isStatic = false;
								return;
							}
							if (speed.y > .18f)
							{
								rigid.velocity = new Vector2(Mathf.Max(Mathf.Min(speed.x * .1f + rigid.velocity.x, maxSpeed), -maxSpeed), Mathf.Min(speed.y * .7f + rigid.velocity.y, maxSpeed));
								ChangeStates(STATE.Flying);
							}
							else
							{
								speed.y = rigid.velocity.y;
								anim.SetFloat(animSpeedHash, Mathf.Abs(speed.x));
								if (Physics2D.OverlapCircle(transform.position, .1f, tiles))
								{
									speed.x *= maxSpeed;
								}
								else
								{
									speed.x = speed.x * .1f + rigid.velocity.x;
									if (speed.x > maxSpeed)
									{
										speed.x = maxSpeed;
									}
									else if (speed.x < -maxSpeed)
									{
										speed.x = -maxSpeed;
									}
								}
								rigid.velocity = speed;
								ChangeStates(STATE.Normal);
							}
							break;
						case STATE.Digging:
							anim.SetFloat(animSpeedHash, speed.y);
							if (StrongPush(speed) != digDirection)
							{
								ChangeStates(STATE.Normal);
								rigid.gravityScale = 1f;
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
				if (state == STATE.Digging || state == STATE.Flying)
				{
					ChangeStates(STATE.Normal);
					audioS.Stop();
				}
				rigid.gravityScale = 1f;
				anim.SetFloat(animSpeedHash, 0f);
			}
			else
			{
				if (Physics2D.OverlapCircle(transform.position, .1f, tiles))
					rigid.velocity = new Vector2(0f, rigid.velocity.y);
				if (contrB.color.a != 0)
				{
					ChangeControllerAlpha(contrB.color.a - .6f);
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
	}

	public void ChangeStates(STATE _state)
	{
		if (_state == state)
		{
			return;
		}
		state = _state;
		if (state == STATE.Flying || (state == STATE.Digging && !Physics2D.OverlapCircle(transform.position, .1f, tiles)))
		{
			flame.SetActive(true);
			audioS.Play();
		}
		else
		{
			flame.SetActive(false);
			audioS.Stop();
		}

		anim.SetInteger(animStateHash, (int)state);
	}

	public void ChangeControllerAlpha(float _alpha)
	{
		Color tempB = contrB.color;
		Color tempJ = contrJ.color;
		tempB.a = _alpha;
		tempJ.a = _alpha;
		contrB.color = tempB;
		contrJ.color = tempJ;
	}

	public void AttackTile()
	{

		defender.hp -= drillStrength;
		Transform defChild = defender.transform.GetChild(0), particlePositioner = transform.GetChild(0);
		dirtParticle.transform.position = particlePositioner.position;
		dirtParticle.transform.rotation = Quaternion.AngleAxis(digDirection, Vector3.forward);
		dirtParticle.Play();
		// dead
		if (defender.hp < 1)
		{
			if (defender.value > (int)MCScript.TILE_VALUES.Treasure)//if it lays minerals
			{
				MCScript.txt.transform.position = Camera.main.WorldToScreenPoint(defender.transform.position);
				MCScript.txt.color = defChild.GetChild(0).GetComponent<SpriteRenderer>().color;
				dropItem.transform.position = defender.transform.position;
				dropItem.Play();
				var temp = dropItem.textureSheetAnimation;
				temp.rowIndex = defender.value - (int)MCScript.TILE_VALUES.Coal;

				notice = ((MCScript.TILE_VALUES)defender.value).ToString() + " +";
				gotItems = true;
			}
			else
			{
				switch ((MCScript.TILE_VALUES)defender.value)
				{
					case MCScript.TILE_VALUES.Dirt:
					case MCScript.TILE_VALUES.Stone:
						break;
					case MCScript.TILE_VALUES.Oil:
						Instantiate(Resources.Load<GameObject>("Oil"),defender.transform.position,Quaternion.identity);
						break;
					case MCScript.TILE_VALUES.Artifact:
						// add to artifacts
						break;
					case MCScript.TILE_VALUES.Map:
						// add to maps
						break;
					case MCScript.TILE_VALUES.Treasure:
						SaveMoneyData savedData = MCScript.GetSavedData();
						savedData.gold += 10000f;
						MCScript.SaveData(savedData);
						MCScript.txt.transform.position = Camera.main.WorldToScreenPoint(defender.transform.position);
						MCScript.txt.color = Color.yellow;
						MCScript.txt.text = "Gold +$$$$10,0000";
						break;
					default:
						break;
				}
			}
			ChangeStates(STATE.Celebrating);
			// check tile above and see if it is placed, and then check lowest undug tile and place it there
			// {

			// }
			MCScript.PlayRandomDeath();
			rigid.gravityScale = 1f;
			Destroy(defender.gameObject);
			return;
		}
		MCScript.PlayRandomAttack();
		int fullHp = MCScript.FullHpForDepth(defender.transform.position.y);
		// set the offset for the crack
		Vector2 offSet = new Vector2(0f, .5f);
		//if we are on the left or right side
		switch (digDirection)
		{
			case 0:
				offSet.y *= defender.transform.position.y - dirtParticle.transform.position.y;
				break;
			case 90:
				offSet.y *= dirtParticle.transform.position.x - defender.transform.position.x;
				break;
			case 180:
				offSet.y *= dirtParticle.transform.position.y - defender.transform.position.y;
				break;
			case 270:
				offSet.y *= defender.transform.position.x - dirtParticle.transform.position.x;
				break;
			default:
				break;
		}
		Material matOfCrack;
		// the damage we are going to do
		int crackCount = Mathf.RoundToInt((drillStrength / (float)fullHp) * 28f);
		// if the tile was at full health
		if (fullHp == defender.hp + drillStrength)
		{
			GameObject crack = Instantiate(Resources.Load<GameObject>("Crack"), defChild.position, Quaternion.AngleAxis(digDirection, Vector3.forward), defChild);
			matOfCrack = crack.GetComponent<Renderer>().material;
			matOfCrack.mainTexture = Resources.Load<Texture>("Crack" + Mathf.Min(28, crackCount));
		}
		else
		{
			// find if there is a crack near where we are hitting
			Material matchingChild = null;
			int i = defChild.childCount; while (--i != 0)
			{
				matchingChild = defChild.GetChild(i).GetComponent<Renderer>().material;
				if (Mathf.Abs(offSet.y - matchingChild.mainTextureOffset.y) < .14f)
				{
					break;
				}
				matchingChild = null;
			}
			if (matchingChild)
			{
				matOfCrack = matchingChild;
				//this grabs the first number in the string
				// we then turn that string into a usable int and add the damage we did
				//string tempstr = Regex.Match(matOfCrack.mainTexture.name, "\\d+").Value;
				float textValue = (fullHp - defender.hp - drillStrength) * 28 / fullHp + crackCount;
				matOfCrack.mainTexture = Resources.Load<Texture>("Crack" + Mathf.Min(28, /*int.Parse(tempstr)*/ textValue));
			}
			else
			{
				// we need a new child
				Transform crack = defChild.GetChild(1);
				matOfCrack = Instantiate(crack.gameObject, crack.position, Quaternion.AngleAxis(digDirection, Vector3.forward), defChild).GetComponent<Renderer>().material;
				matOfCrack.mainTexture = Resources.Load<Texture>("Crack" + crackCount);
			}
		}
		matOfCrack.mainTextureOffset = offSet;
		StartCoroutine(Shake(defChild, .5f, .3f));
	}

	public void FinishCelebrating()
	{
		ChangeStates( STATE.Normal);
	}

	//int AngleOfHit_90s(Vector2 _dir)
	//{
	//	if (Mathf.Abs(_dir.x) > Mathf.Abs(_dir.y))
	//		return _dir.x > 0 ? 0 : 180;
	//	else
	//		return _dir.y > 0 ? 90 : 270;
	//}

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

	int StrongPush(Vector2 _dir)
	{
		const float minV = .707f;
		const float maxV = -minV;
		if (_dir.x > minV)
			return 0;
		if (_dir.x < maxV)
			return 180;
		if (_dir.y < maxV)
			return 270;
		if (_dir.y > minV)
			return 90;
		return -1;
	}

	public bool GetTile(float _x, float _y)
	{

		Vector2 hitPoint = transform.position;
		hitPoint.x += _x;
		hitPoint.y += _y;
		//debugger.position = hitPoint;
		Collider2D hitCollider = Physics2D.OverlapPoint(hitPoint, tiles);
		if (hitCollider)
		{
			Tile temp = hitCollider.GetComponent<Tile>();
			if (temp.value != (byte)MCScript.TILE_VALUES.Stone)
			{
				defender = temp;
				return true;
			}
		}
		defender = null;
		return false;
	}

	bool CheckDig(int _dir)
	{
		switch (_dir)
		{
			case -1:
				defender = null;
				return false;
			case 90:
				return GetTile(0f, 1.5f);
			case 0:
				return GetTile(.5f, .63f);
			case 270:
				return GetTile(.35f * Mathf.Sign(transform.localScale.x), -.05f);
			case 180:
				return GetTile(-.5f, .63f);
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
		yield break;
	}
}