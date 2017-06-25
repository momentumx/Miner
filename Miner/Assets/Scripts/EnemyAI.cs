using UnityEngine;
using System.Collections.Generic;

[RequireComponent ( typeof ( Animator ) )]
[RequireComponent ( typeof ( SphereCollider ) )]
public class EnemyAI : Health {
	enum PHYSIQUE {
		TwoD,
		ThreeD
	}
	[SerializeField]
	PHYSIQUE physic;
	[SerializeField]
	LayerMask detectLayers;
	//set in unity
	public AudioClip audio_attack, audio_dead;
	Collider doubleDamg;
	public string[] tags;
	[HideInInspector]
	public uint dotB, dotF, dotP, occupiedbit;
	public int damage;
	public float attackRate, speed, atkrange, rotSpd;
	protected uint timerB, timerP, timerF;
	protected Health targetScript;
	Vector3 dir, flipScale;
	protected Transform target;
	[HideInInspector]
	public float nextAttack, offx, offz;
	[HideInInspector]
	public Animator m_Animator;
	protected short attacks;
	List<Color> resetColor;
	List<Vector3> wayPoints;
	[SerializeField]
	protected MasterControllerScript.SAVEKEYINT saveKey;

	public override void Start () {
		base.Start ();
		atkrange += radius;
		atkrange *= atkrange;
		flipScale = transform.localScale;
		// get the components on the object we need ( should not be null due to require component so no need to check )
		m_Animator = GetComponentInChildren<Animator> ();
		GetNearestTarget ();
		resetColor = new List<Color> ();
		wayPoints = new List<Vector3> ();
		resetColor.Add ( GetComponent<Renderer> ().material.color );
	}

	virtual protected void Attack () {
		m_Animator.SetTrigger ( "attacking" );
		nextAttack = Time.time + attackRate;
	}

	void DealDamage ()//called from animator
	{
		if ( !targetScript || currhp == 0 || targetScript.currhp == 0 )
			return;
		if ( audio_attack )
			MasterControllerScript.sfxPlayer.PlayOneShot ( audio_attack );
		else
			MasterControllerScript.sfxPlayer.PlayOneShot (
				MasterControllerScript.audio_attacks [ Random.Range ( 0, MasterControllerScript.audio_attacks.Length ) ] );
		targetScript.TakeDamage ( damage, Color.red );
		//if target is moving
		if ( GetDistanceSqd2D ( target.position.x + offx, target.position.z + offz ) > atkrange ) {
			m_Animator.SetBool ( "moving", true );
		}
	}

	void ClearDOT ( Color _color, ref uint _dot ) {
		resetColor.Remove ( _color );
		GetComponent<Renderer> ().material.color = resetColor [ 0 ];
		_dot = 0;
	}

	bool TimeCount ( ref uint _timer, Color _color, ref uint _dot ) {
		--_timer;
		if ( _timer % 60 == 1 ) {
			TakeDamage ( ( int )_dot, _color );
			if ( _timer == 0 ) {
				ClearDOT ( _color, ref _dot );
				return true;
			}
		}
		return false;
	}

	public override void FixedUpdate () {
		//healthbar stuff
		base.FixedUpdate ();
		//similates friction after death
		if ( currhp == 0 ) {
			if ( speed > 0f )
				speed -= .1f;
			else speed = 0;
			transform.position += dir * speed;
			return;
		}

		if ( timerB != 0 )
			TimeCount ( ref timerB, Color.red, ref dotB );

		if ( timerF != 0 && TimeCount ( ref timerF, Color.blue, ref dotF ) ) {
			speed *= 2;
			attackRate *= .5f;
		}

		if ( timerP != 0 && TimeCount ( ref timerP, Color.green, ref dotP ) )
			Destroy ( transform.GetChild ( 1 ).gameObject );
		//if target is destroyed / dead / or if its full and i didnt help make it full
		if ( targetScript == null || targetScript.currhp == 0 || targetScript.occupiedbits == targetScript.avaiablespaces ){
			GetNearestTarget ();
			return;
		}

		if ( physic == PHYSIQUE.TwoD ) {
			Vector3 lookat = camerapos.position;
			lookat.y = 0;
			transform.LookAt ( lookat );
			float angle = Vector3.Dot((target.position - transform.position), camerapos.right);
			if ( Mathf.Abs ( angle ) > .1f ) {// this allows some leeway when walking right at 90
				flipScale.x = Mathf.Abs ( flipScale.x ) * Mathf.Sign ( angle );
				transform.localScale = flipScale;
			}
		}
		if ( GetDistanceSqd2D ( target.position.x + offx, target.position.z + offz, ref dir ) > atkrange ) {
			//might want to change the 5 to soemthing that is related to the enemies turn radius
			if ( occupiedbit == 0u && dir.sqrMagnitude < targetScript.radius*targetScript.radius * 10f )
				GetOpenSpot ();//this sets occupied bit to something other than 0 and sets offs to a value
			if ( physic == PHYSIQUE.ThreeD ) {
				RaycastHit rayHit;
				if ( Physics.SphereCast ( transform.position, radius , transform.forward, out rayHit, radius * 5f, detectLayers )) {
					if ( Vector3.Dot ( transform.right, rayHit.transform.position - transform.position ) > 0f )
						transform.rotation = Quaternion.Slerp ( transform.rotation, Quaternion.LookRotation ( Vector3.Cross ( Vector3.up, rayHit.normal ) ), rotSpd );
					else
						transform.rotation = Quaternion.Slerp ( transform.rotation, Quaternion.LookRotation ( Vector3.Cross ( Vector3.down, rayHit.normal ) ), rotSpd );
				} else {
					if ( !Physics.SphereCast ( transform.position, radius, target.position - transform.position, out rayHit, radius * 5f, detectLayers ) )
						transform.rotation = Quaternion.Slerp ( transform.rotation, Quaternion.LookRotation ( dir ), rotSpd );
				}
				transform.position += transform.forward * speed;

			} else {
				dir.Normalize ();
				transform.position += dir * speed;
			}
		}
		else {
			if ( physic == PHYSIQUE.ThreeD )
				transform.rotation = Quaternion.Slerp ( transform.rotation, Quaternion.LookRotation ( dir ), rotSpd );
			if ( Time.time > nextAttack )
				Attack ();//in case they do soemthing special when attacking
		}
	}


	protected float GetDistanceSqd2D ( float _x, float _z, ref Vector3 _dir ) {
		_dir.x = _x - transform.position.x;
		_dir.z = _z - transform.position.z;
		return ( dir.x * dir.x + dir.z * dir.z );
	}

	protected float GetDistanceSqd2D ( float _x, float _z ) {
		return ( _x * _x + _z * _z );
	}

	void OnTriggerExit ( Collider coll ) {
		if ( currhp != 0 ) {
			if ( timerF != 0 && coll.tag == "Frost" ) {
				timerF = 0;
				ClearDOT ( Color.blue, ref dotF );
				speed *= 2;
				attackRate *= .5f;
			}
			if ( timerB != 0 && coll.tag == "Napalm" ) {
				timerB = 0;
				ClearDOT ( Color.red, ref dotB );

			}
		}
	}

	//take damage
	void OnTriggerEnter ( Collider coll ) {
		if ( currhp != 0 ) {
			if ( coll.tag == "Frost" ) {
				dotF += 1u;
				if ( timerF == 0u ) {
					resetColor.Insert ( 0, Color.blue );
					speed *= .5f;
					attackRate *= 2f;
				}
				timerF = 400u;
				GetComponent<SpriteRenderer> ().color = Color.blue;
				return;
			}

			if ( coll.tag == "Poison" ) {
				dotP += 1u;
				if ( timerP == 0u ) {
					Instantiate ( Resources.Load<GameObject> ( "ImPoisened" ), transform.position, Quaternion.identity, transform );
					resetColor.Insert ( 0, Color.green );
				}
				timerP = 400u;
				GetComponent<SpriteRenderer> ().color = Color.green;
				return;
			}

			if ( coll.tag == "Napalm" ) {
				dotB += 1u;
				if ( timerB == 0u ) {
					Instantiate ( Resources.Load<GameObject> ( "ImPoisened" ), transform.position, Quaternion.identity, transform );
					resetColor.Insert ( 0, Color.red );
				}
				timerB = 400u;
				GetComponent<SpriteRenderer> ().color = Color.red;
				return;
			}
			if ( coll.tag == "Shield" ) {
				transform.position = coll.transform.position + ( transform.position - coll.transform.position ).normalized * coll.bounds.extents.x;
				return;
			}
			if ( coll.tag == "Convert" && HP < 100 ) {
				GetComponent<SpriteRenderer> ().color = Color.magenta;
				resetColor.Insert ( resetColor.Count - 1, Color.magenta );
				//WaveControlScript.enemies.Remove ( this );
				//WaveControlScript.converted.Add ( this );
				tags = new string [ 1 ] { "Enemy" };
				GetNearestTarget ();
				return;
			}
			if ( coll.tag == "Blackhole" && HP < 100 ) {
				foreach ( ParticleSystem parts in coll.GetComponentsInChildren<ParticleSystem> () )
					parts.Play ();
				Destroy ( coll.gameObject, .5f );
				coll.tag = "Untagged";

				//WaveControlScript.enemies.Remove ( this );
				//++Player.shotsHit;
				//++Player.enemiesKilled;
				MasterControllerScript.SaveKeyInc ( saveKey );
				Destroy ( gameObject );
				return;
			}
			if ( coll.tag == "Spell" && doubleDamg != coll ) {
				//TakeDamage ( Mathf.CeilToInt ( coll.GetComponent<ProjectileScript> ().damage ), Color.red, coll.GetComponent<ProjectileScript> ().magnitude );
				doubleDamg = coll;
			}
		}
	}

	public override void TakeDamage ( int _dmg, Color _color, float _magnitude = 1 ) {
		if ( currhp != 0 ) {
			//++Player.shotsHit;
			base.TakeDamage ( _dmg, _color );
			if ( currhp == 0 ) {
				//++Player.enemiesKilled;
				if ( targetScript )
					targetScript.occupiedbits &= ~occupiedbit;
				//WaveControlScript.enemies.Remove ( this );
				currhp = 0;
				float angle = Random.Range(0, 6.28f);
				dir.x = transform.position.x + Mathf.Cos ( angle ) - transform.position.x;
				dir.z = transform.position.z + Mathf.Sin ( angle ) - transform.position.z;
				speed = 5f * _magnitude;
				if ( audio_dead )
					MasterControllerScript.sfxPlayer.PlayOneShot ( audio_dead );
				else
					MasterControllerScript.sfxPlayer.PlayOneShot ( MasterControllerScript.audio_deaths [ Random.Range ( 0, MasterControllerScript.audio_deaths.Length ) ] );
				m_Animator.SetTrigger ( "dying" );
			}
		}
	}


	// and finally the actual process for finding the nearest object:
	virtual public void GetNearestTarget () {
		target = null;
		m_Animator.speed = 0f;
		float distance = Mathf.Infinity;
		occupiedbit = 0u;
		offx = offz = 0f;

		// loop through the  buildings, remembering nearest one found
		//foreach ( Health building in Player.buildings ) {
		//	bool unwanted = true;
		//	sbyte i = -1; while ( ++i != tags.Length )
		//		if ( building.tag == tags [ i ] ) { unwanted = false; break; }
		//
		//	if ( unwanted )
		//		continue;
		//	//checks to see if target has slots open and that it is alive
		//	if ( building.occupiedbits != building.avaiablespaces && building.currhp != 0 ) {
		//		float thisdistance = GetDistanceSqd2D(transform.position.x - building.transform.position.x,transform.position.z - building.transform.position.z);
		//		if ( thisdistance < distance ) {
		//			target = building.transform;
		//			distance = thisdistance;
		//			targetScript = building;
		//		}
		//	}
		//}
		if ( target ) {
			m_Animator.speed = 1f;
			RaycastHit rayHit;
			wayPoints.Add ( target.position );
			Vector3 dis = target.position - transform.position;
			if ( Physics.SphereCast ( transform.position, radius, dis, out rayHit, dis.magnitude - targetScript.radius*2f, detectLayers ) ) {
				Vector3 right = Vector3.Cross(rayHit.normal, Vector3.up);
				float width;
				if(Vector3.Dot(right, rayHit.transform.forward) < float.Epsilon ) {
					width = rayHit.collider.bounds.extents.x;
				} else {
					width = rayHit.collider.bounds.extents.z;
				}
			}

		}
	}

	virtual public void GetOpenSpot () {
		float circum = 2f*Mathf.PI;
		float rads = (circum + Mathf.Atan2(transform.position.z - target.position.z, transform.position.x - target.position.x))%circum;
		int bitShift = (int)(targetScript.slots* rads/circum);
		occupiedbit = 1u << bitShift;
		//if spot is not taken
		if ( ( ( 1u << bitShift ) & targetScript.occupiedbits ) != 0u ) {
			int i = 0; while ( ++i != targetScript.slots ) {
				if ( (i+bitShift) < targetScript.slots && ( targetScript.occupiedbits & ( 1u << ( i + bitShift ) ) ) == 0u ) {
					occupiedbit = 1u << ( i + bitShift );
					rads = ( i + bitShift ) / ( float )targetScript.slots;
					break;
				}
				if ( (bitShift - i )>0 && ( targetScript.occupiedbits & ( 1u << ( bitShift - i ) ) ) == 0u ) {
					occupiedbit = 1u << ( bitShift - i );
					rads = ( bitShift - i ) / ( float )targetScript.slots;
					break;
				}
			}

		}
		offx = targetScript.radius * Mathf.Cos ( rads );
		offz = targetScript.radius * Mathf.Sin ( rads );
		targetScript.occupiedbits |= occupiedbit;
		m_Animator.SetBool ( "moving", true );
	}


	virtual public void Die ()//called form animator
	{
		if ( physic == PHYSIQUE.TwoD ) {
			SpriteRenderer deadSprite = new GameObject("deadcopy", typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();//create a gameobject with a sprite renderer
			SpriteRenderer mysprite = GetComponent<SpriteRenderer>();
			deadSprite.sprite = mysprite.sprite;
			deadSprite.transform.position = new Vector3 ( transform.position.x, .1f, transform.position.z );
			deadSprite.transform.localScale = transform.localScale;
			Vector3 lookat = camerapos.position;
			lookat.y = 1000f;
			transform.LookAt ( lookat );
			deadSprite.transform.rotation = transform.rotation;
			deadSprite.flipX = mysprite.flipX;
			deadSprite.color = mysprite.color;
			deadSprite.material = mysprite.material;
			deadSprite.sortingOrder = -2;
		}
		MasterControllerScript.SaveKeyInc ( saveKey );
		Destroy ( gameObject );
	}
}