using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
	public enum STATE { Normal, Digging, Celebrating, Dying, Breaking, Flying, WalkIn }
	public STATE state = STATE.Flying;
	public int digDirection;
	[SerializeField]
	UnityEngine.UI.RawImage contrB, contrJ;
	Rigidbody2D rigid;
	Animator anim;
	[SerializeField]
	GameObject flame;
	[SerializeField]
	float maxSpeed = .4f,  xSpeed = .3f, ySpeed = .1f, sqdDragDist=2000;
	public Tile defender;
	[SerializeField]
	LayerMask tiles, ground;
	int animSpeedHash, animStateHash;
	[SerializeField]
	ParticleSystem dirtParticle, dropItem, dieParticle;
	AudioSource audioS;
	static public List<TypeAmount> bag = new List<TypeAmount>();
	static public ushort[] items = new ushort[MCScript.differentItemCount];
	public static Transform target;
	public static sbyte drillStrength = 1;
	public bool showJoyStick;
	public static bool dragBegan, alive = true;


	// Use this for initialization
	void Start()
	{
		digDirection = -1;
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
		if (CameraMovement.cameraMode == CameraMovement.CAMERA_MODE.UnderGround && alive)
		{
			if (transform.position.y > 1.1f)
			{// return
				GoUp();
				return;
			}
			Vector2 rayPos = transform.position;
			rayPos.y += 1f;
			RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, 500f, LayerMask.GetMask("Stone"));
			if (hit)
			{
				Debug.Log(hit.transform.name);
				rayPos.y -= 1f;
				if (Physics2D.Raycast(rayPos, Vector2.zero))
				{
					Die();
				}
			}
			if (Input.GetMouseButtonDown(0))
			{
				BeginClick();
			}
			if (target)
			{
				Vector2 autoDir = (Vector2)target.position - (Vector2)transform.position;
				autoDir.x *= 600f;
				autoDir.y *= 200;
				Moving(Vector2.ClampMagnitude(autoDir, 100f));
			}
			else
			{
				if (Input.GetMouseButton(0))
				{
					Vector2 speed = Vector2.ClampMagnitude((Vector2)Input.mousePosition - (Vector2)contrB.transform.position, 100f);

					if (dragBegan)
					{
						Moving(speed);
					}
					else
					{
						Clicking(speed);
					}
				}
				else if (Input.GetMouseButtonUp(0))
				{
					EndClick();
				}
				else
				{
					if (Physics2D.OverlapCircle(transform.position, .1f, ground))
						rigid.velocity = new Vector2(0f, rigid.velocity.y);
					if (contrB.color.a != 0)
					{
						ChangeControllerAlpha(contrB.color.a - Time.deltaTime);
					}
				}
			}
		}
	}

	private void Die()
	{
		alive = false;
		CoppyScript.coppy.writer.AddMessage("Ouch! You got squashed!");
		CoppyScript.coppy.writer.AddMessage("I'll bring you back to base, but try and avoid those falling rocks!",GoUp);
		transform.GetChild(1).gameObject.SetActive(false);
		dieParticle.Play();
	}

	public void GoUp(System.Object obj = null)
	{
		rigid.velocity = Vector2.zero;
		rigid.gravityScale = 0f;
		CameraMovement.cam.GoVisit();
		alive = true;
		transform.GetChild(1).gameObject.SetActive(true);
		ChangeStates(STATE.WalkIn);// play the enter animation
		ChangeControllerAlpha(0f);// the controller only works underground so we need to turn it off here
		audioS.Stop();// stop the flame noise
					  // make sure that we are facing to the right
		Vector3 newScale = transform.localScale;
		newScale.x = Mathf.Abs(transform.localScale.x);
		transform.localScale = newScale;
		// turn off shadow flicker
		transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
	}

	public void BeginClick()
	{
		contrB.transform.position = Input.mousePosition;
		contrJ.transform.position = Input.mousePosition;
		dragBegan = false;
		target = null;
	}

	public void Clicking(Vector2 speed)
	{
		if (speed.sqrMagnitude > sqdDragDist)
		{
			if (showJoyStick)
			{
				ChangeControllerAlpha(.5f);
			}
			dragBegan = true;
		}
	}

	public void Moving(Vector2 speed)
	{
		speed *= .01f;// this brings the x & y back to a number between 0-1;
		contrJ.transform.localPosition = speed * 250f; // this allows us to scale the distance the joystick can travel without effecting the speed of everything else, remove th float when we have the actual image size
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
					CreatStuff();
					ChangeStates(STATE.Digging);
					rigid.velocity = Vector2.zero;
					rigid.gravityScale = 0f;
					//defender.gameObject.isStatic = false;
					return;
				}
				if (speed.y > .1f)
				{
					rigid.velocity = new Vector2(
					Mathf.Max(// X
						Mathf.Min(speed.x * xSpeed + rigid.velocity.x, maxSpeed + 2f)
						, -maxSpeed - 2f)
					, Mathf.Min(// Y
						speed.y * ySpeed + rigid.velocity.y
						, maxSpeed+2f)
					);
					ChangeStates(STATE.Flying);
				}
				else
				{
					speed.y = rigid.velocity.y;
					anim.SetFloat(animSpeedHash, Mathf.Abs(speed.x));
					if (Physics2D.OverlapCircle(transform.position, .1f, ground))
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
				if (flame.activeSelf)
				{
					rigid.velocity = Vector2.zero;
				}
				break;
			case STATE.Dying:
				break;
			case STATE.Breaking:
				break;
			default:
				break;
		}
	}

	public void EndClick()
	{
		if (state == STATE.Digging || state == STATE.Flying)
		{
			ChangeStates(STATE.Normal);
			audioS.Stop();
		}
		rigid.gravityScale = 1f;
		anim.SetFloat(animSpeedHash, 0f);
	}

	public Vector2 GetVectorForApp()
	{
		if (target)
		{
			return (target.transform.position - transform.position).normalized * ySpeed;
		}

		return Vector2.zero;
	}

	public void ChangeStates(STATE _state)
	{
		if (_state == state)
		{
			return;
		}
		state = _state;
		if (state == STATE.Flying || (state == STATE.Digging && !Physics2D.OverlapCircle(transform.position, .1f, ground)))
		{
			flame.SetActive(true);
			audioS.Play();
		}
		else
		{
			if (state != STATE.Celebrating)
			{
				flame.SetActive(false);
				audioS.Stop();
			}
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
		CheckDig(digDirection);
		if (defender == null)
		{
			return;
		}
		defender.hp -= drillStrength;
		Transform defChild = defender.transform.GetChild(0), particlePositioner = transform.GetChild(0);
		dirtParticle.transform.position = particlePositioner.position;
		dirtParticle.transform.rotation = Quaternion.AngleAxis(digDirection, Vector3.forward);
		dirtParticle.Play();
		// dead
		if (defender.hp < 1)
		{
			if (defender.transform == target)
			{
				target = null;
				anim.SetFloat(animSpeedHash, 0f);
			}
			MCScript.TILE_VALUES tileValue = (MCScript.TILE_VALUES)defender.value;
			if (tileValue > MCScript.TILE_VALUES.Treasure)//if it lays minerals
			{
				int randomAmount = UnityEngine.Random.Range(8, 16);
				UnityEngine.UI.Text mineralText = GameObject.Find("MineralCountTxt").GetComponent<UnityEngine.UI.Text>();
				int bagCount = int.Parse(System.Text.RegularExpressions.Regex.Match(mineralText.text, "\\d+").Value);
				if (randomAmount + bagCount >= MCScript.savedBothData.bagSize)
				{
					randomAmount = (int)MCScript.savedBothData.bagSize - bagCount;
					MCScript.SetText("Bag Full\n" + (tileValue).ToString() + " +" + randomAmount, Color.red, Camera.main.WorldToScreenPoint(defender.transform.position));
				}
				else
				{
					MCScript.SetText((tileValue).ToString() + " +" + randomAmount, defChild.GetChild(0).GetComponent<SpriteRenderer>().color, Camera.main.WorldToScreenPoint(defender.transform.position));
				}
				if (randomAmount > 0)
				{
					dropItem.transform.position = defender.transform.position;
					var temp = dropItem.textureSheetAnimation;
					temp.rowIndex = (tileValue - MCScript.TILE_VALUES.Coal)/10;
					temp.startFrame = (tileValue - MCScript.TILE_VALUES.Coal)%10;
					dropItem.Emit(randomAmount);
					mineralText.text = (bagCount + randomAmount).ToString() + '/' + MCScript.savedBothData.bagSize;

					uint index = (uint)(tileValue - MCScript.TILE_VALUES.Coal);
					int match = bag.IndexOf(index);
					if (match == -1)
					{
						bag.Add(new TypeAmount(index, (uint)randomAmount));
					}
					else
					{
						bag[match] += (uint)randomAmount;

					}
				}
				ChangeStates(STATE.Celebrating);
			}
			else
			{
				switch ((MCScript.TILE_VALUES)defender.value)
				{
					case MCScript.TILE_VALUES.Dirt:
						ChangeStates(STATE.Normal);
						break;
					case MCScript.TILE_VALUES.Oil:
						ChangeStates(STATE.Normal);
						Instantiate(Resources.Load<GameObject>("Oil"), defender.transform.position, Quaternion.identity);
						break;
					case MCScript.TILE_VALUES.Artifact:
						ChangeStates(STATE.Celebrating);
						// add to artifacts
						break;
					case MCScript.TILE_VALUES.Map:
						ChangeStates(STATE.Celebrating);
						++items[3];
						dropItem.transform.position = defender.transform.position;
						dropItem.Emit(1);
						var temp = dropItem.textureSheetAnimation;
						temp.rowIndex = (int)MCScript.COLLECTIBLES.Marsinium+1;
						break;
					case MCScript.TILE_VALUES.Treasure:
						MCScript.ChangeGold(10000f, Camera.main.WorldToScreenPoint(defender.transform.position));
						ChangeStates(STATE.Celebrating);
						break;
					default:
						break;
				}
			}
			RaycastHit2D hitInfo = Physics2D.Raycast((Vector2)defender.transform.position + Vector2.up * 2f, Vector2.zero);
			Tile upNeighbor;
			while (hitInfo)
			{
				upNeighbor = hitInfo.collider.GetComponent<Tile>();
				if (upNeighbor)
				{
					if ((MCScript.TILE_VALUES)upNeighbor.value == MCScript.TILE_VALUES.Stone)
					{
						if (!upNeighbor.GetComponent<Rigidbody2D>())
						{
							upNeighbor.gameObject.isStatic = false;
							Rigidbody2D newBody = upNeighbor.gameObject.AddComponent<Rigidbody2D>();
							newBody.freezeRotation = true;
							newBody.mass = 10000f;
						}
					}
					hitInfo = Physics2D.Raycast((Vector2)upNeighbor.transform.position + Vector2.up * 2f, Vector2.zero);
				}
				else
				{
					break;
				}
			}
			PersistentManager.PlayRandomDeath();
			rigid.gravityScale = 1f;
			Destroy(defender.gameObject);
			return;
		}
		PersistentManager.PlayRandomAttack();
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
		ChangeStates(STATE.Normal);
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

	bool GetTile(float _x, float _y)
	{

		Vector2 hitPoint = transform.position;
		hitPoint.x += _x;
		hitPoint.y += _y;
		//debugger.position = hitPoint;
		Collider2D hitCollider = Physics2D.OverlapPoint(hitPoint, tiles);
		if (hitCollider)
		{
			// guaranteed to have a tile script bc of layermask
			defender = hitCollider.GetComponent<Tile>();
			return true;
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

	public void CreatStuff()
	{

		int gridEndPos = Mathf.Max(0, ((int)(transform.position.x) >> 1) - 4/*width*/);
		int section = gridEndPos / 32;
		if (section < 3)
		{
			uint bitShift;
			int y, offset;
			int shiftAmount = gridEndPos % 32 - 1;
			int x = Mathf.Min(32, shiftAmount + 15);
			int yStartIndex = Mathf.Max(-1, ((int)transform.position.y) +10/*we only need to go up a little bc the camera is low*/);
			int yEndIndex = Mathf.Min(999, yStartIndex + (int)Camera.main.orthographicSize + 10/*height*2 + 1*/);
			SaveBelowData savedBelow = MCScript.SavedBelowData;
			Vector2 gridPos;

			offset = 64 * section;
			while (--x != shiftAmount)
			{
				bitShift = ((0x80000000) >> x);
				y = yStartIndex; while (++y != yEndIndex)
				{
					if ((savedBelow.tiles[section, y] & bitShift) == 0)
					{
						savedBelow.tiles[section, y] |= bitShift;
						gridPos.x = 2 * x + offset;// 2 is the size of the objects (200 pixles, and the world is set to 100 pixels per unit)
						gridPos.y = -2 * y;
						MCScript.CreateTile(gridPos, y);
					}
				}
			}
			if (shiftAmount > 23 && section < 2)
			{
				++section;
				x = -1;
				shiftAmount -= 23;
				offset = 64 * section;
				while (++x != shiftAmount)
				{
					bitShift = ((0x80000000) >> x);
					y = yStartIndex; while (++y != yEndIndex)
					{
						if ((savedBelow.tiles[section, y] & bitShift) == 0)
						{
							savedBelow.tiles[section, y] |= bitShift;
							gridPos.x = 2 * x + offset;
							gridPos.y = -2 * y;
							MCScript.CreateTile(gridPos, y);
						}
					}
				}
			}
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
			offSet.x = UnityEngine.Random.Range(-1f, 1f) * damper;
			offSet.y = UnityEngine.Random.Range(-1f, 1f) * damper;

			_trans.localPosition = offSet;
			_trans.localRotation = Quaternion.AngleAxis(UnityEngine.Random.Range(-45f, 45f) * damper, _trans.forward);

			elapsed += Time.deltaTime;
			yield return null;
		}
		if (_trans)
			_trans.localPosition = Vector2.zero;
		yield break;
	}
}