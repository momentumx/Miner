using System.Collections;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
	Vector2 startPos, endPos;
	Animator anim;
	Coroutine movingToTarget;
	float timeHeld;
	public float timeOfDash, spdOfDash, minEnemyDis, minDragToAtk;
	public GameObject[] enemies;
	// Start is called before the first frame update
	void Start()
	{
		anim = GetComponent<Animator>();
	}

	// Update is called once per frame
	void Update()
	{
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
#endif
#if UNITY_EDITOR
		//debug stuff
#endif
#if UNITY_ANDROID || UNITY_IOS
		if (Input.GetMouseButtonDown(0))
		{
			MouseDown();
		}
		else if (Input.GetMouseButtonUp(0))
		{
			MouseUp();
		}
		else if (Input.GetMouseButton(0))
		{
			MouseIsDown();
		}
#endif
	}

	public void MouseDown()
	{
		startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		anim.Play("Charge");
	}



	public void MouseIsDown()
	{
		// maybe draw some line and store for later travel
		timeHeld += Time.deltaTime;
	}



	public void MouseUp()
	{
		endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector2 dir = endPos - startPos;
			// long enough to attack
		if (dir.sqrMagnitude > minDragToAtk)
		{
			Vector2 targetDest = ((Vector2)transform.position) + dir;
			float smallestDis = minEnemyDis, dis;
			GameObject closeEnemy = null;
			foreach (GameObject enemy in enemies)
			{
				if (enemy)
				{
					dis = (targetDest - (Vector2)enemy.transform.position).sqrMagnitude;
					if(dis < smallestDis)
					{
						smallestDis = dis;
						closeEnemy = enemy;
					}
				}
			}

			if (closeEnemy)
			{
				dir = closeEnemy.transform.position - transform.position;
			}

			if (movingToTarget != null)
			{
				StopCoroutine(movingToTarget);
			}
			anim.Play("Attack");
			movingToTarget = StartCoroutine(DragMeToSpot(dir.normalized));
		}
		else
		{
			anim.Play("Walk");
		}

		timeHeld = 0f;
	}

	IEnumerator DragMeToSpot(Vector2 dir)
	{
		float timeLeft = timeOfDash;
		Vector2 dest = transform.position * spdOfDash * timeLeft;
		while (timeLeft > 0 && (dest - (Vector2)transform.position).sqrMagnitude>2f)
		{
			transform.position += (Vector3)(dir * Time.deltaTime * spdOfDash);
			timeLeft -= Time.deltaTime;
			yield return null;
		}
		movingToTarget = null;
		anim.Play("Walk");
		yield break;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		StopCoroutine(movingToTarget);
		anim.Play("Walk");
		Destroy(collision.gameObject);
	}
}
