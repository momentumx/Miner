using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour
{

	public enum CAMERA_MODE { Transition = -1, Above, UnderGround, Map, AboveSlide }
	public static CAMERA_MODE cameraMode = 0;
	public static CAMERA_MODE oldMode = 0;

	PlayerScript target;
	Vector2 lastPos, speed;
	const float acc = .93f;
	public float width, mapModeSpd = .02f;
	public GameObject upperMenu, mapMenu, lowerMenu;
	static public CameraMovement cam;

	void Start()
	{
		cam = GetComponent<CameraMovement>();
		target = FindObjectOfType<PlayerScript>();
	}

	void Update()
	{
		switch (cameraMode)
		{
			case CAMERA_MODE.AboveSlide:
			case CAMERA_MODE.Above:
				if (Input.GetMouseButton(0))
				{
					if (Input.GetMouseButtonDown(0))
					{
						lastPos = Input.mousePosition;
					}
					else
					{
						speed = (Vector2)Input.mousePosition - lastPos;
						speed.y = 0f;
						speed.x *= .1f;
						lastPos = Input.mousePosition;
					}
				}

				else
				{
					speed.x *= acc;
				}
				transform.position += (Vector3)speed;
				break;
			case CAMERA_MODE.UnderGround:
				transform.position += (Vector3)(((Vector2)target.transform.position - (Vector2)transform.position) * .04f);
				break;
			case CAMERA_MODE.Map:
				Camera.main.orthographicSize -= Input.GetAxis("WheelZoom");
				Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 6f, 30f);
				if (Input.GetMouseButton(0))
				{
					if (Input.GetMouseButtonDown(0))
					{
						lastPos = Input.mousePosition;
					}
					else
					{
						speed = (Vector2)Input.mousePosition - lastPos;
						transform.position += (Vector3)speed * mapModeSpd;
						lastPos = Input.mousePosition;
					}
				}
				float topY = -30 * Screen.height / (Screen.width * 2) - 2f;
				if (transform.position.y > topY)
					transform.position = new Vector3(transform.position.x, topY);
				break;
			default:
				break;
		}
		float camPosX = transform.position.x;
		if (camPosX > width)
			transform.position = new Vector3(width, transform.position.y);
		else
		{
			float sWidth = Camera.main.orthographicSize * Screen.width / Screen.height - .75f;
			if (camPosX < sWidth)
				transform.position = new Vector3(sWidth, transform.position.y);
		}
	}

	// called from animator after the transition
	public void Up()
	{
		cameraMode = CAMERA_MODE.Transition;
		oldMode = CAMERA_MODE.Above;
		MCScript.mcScript.GoUp();
	}

	// Used when we are actually going up
	public void GoVisit()
	{
		target.transform.position = new Vector2(.3f, 2.45f);
		bool goingDown = transform.position.y == 1f;
		if (goingDown)
		{
			CoppyScript.coppy.depthTxt.transform.parent.gameObject.SetActive(true);
			target.ChangeStates(PlayerScript.STATE.Normal);
			target.GetComponent<Rigidbody2D>().gravityScale = 0f;
		}
		else
		{
			CoppyScript.coppy.depthTxt.transform.parent.gameObject.SetActive(false);
			target.ChangeStates(PlayerScript.STATE.WalkIn);
			upperMenu.SetActive(true);
		}
		GoView(Camera.main.orthographicSize * Screen.width / Screen.height - .75f, goingDown);
	}

	// used when we are just peeking (not able to select buttons)
	public void GoView(float _x, bool _goingDown)
	{
		cameraMode = CAMERA_MODE.Transition;
		StartCoroutine(MoveToPos(_x, () => SetupPlayer(_goingDown)));
	}

	//public void SetCameraMode(CAMERA_MODE _mode)
	//{
	//	oldMode = cameraMode;
	//	cameraMode = _mode;
	//}

	public void SetCameraBack()
	{
		cameraMode = oldMode;
	}

	public void SetCameraOldMode(int _mode)
	{
		oldMode = (CAMERA_MODE)_mode;
	}

	public void SetCameraMode(int _mode)
	{
		cameraMode = (CAMERA_MODE)_mode;
	}

	public void GoMap(bool startMap)
	{
		cameraMode = CAMERA_MODE.Transition;
		StartCoroutine(MoveToPos(Camera.main.orthographicSize * Screen.width / Screen.height - .75f, () => SetupMap(startMap)));
	}

	static public void Win()
	{

	}

	static public void Lose()
	{

	}

	public IEnumerator MoveToPos(float _x, System.Action functionCall, float _y = 1f, float spd = .1f)
	{
		Vector2 pos = new Vector2(_x, _y);
		while (Vector2.SqrMagnitude(pos - (Vector2)transform.position) > .4f)
		{
			transform.position = Vector2.LerpUnclamped(transform.position, pos, spd);
			yield return null;
		}
		transform.position = pos;
		functionCall?.Invoke();
		yield break;
	}

	public IEnumerator ZoomOrtho(float zoom, float spd = .1f)
	{
		while (Camera.main.orthographicSize < zoom)
		{
			Camera.main.orthographicSize += spd;
			yield return null;
		}
		Camera.main.orthographicSize = zoom;
		yield break;
	}

	public void SetupPlayer(bool _goinDown)
	{
		if (_goinDown)
		{
			target.GetComponent<Rigidbody2D>().gravityScale = 1f;
		}
		GetComponent<Animator>().SetBool("Up", !_goinDown);
	}

	public void SetupMap(bool startMAp)
	{
		FindObjectOfType<PlayerScript>().ChangeControllerAlpha(0f);
		upperMenu.SetActive(!startMAp);
		mapMenu.SetActive(startMAp);
		StartCoroutine(MoveToPos(30 * Screen.width / Screen.height - .75f,() => SetCameraMode(2), -30 * Screen.height / (Screen.width*2)-2f,.1f));
		StartCoroutine(ZoomOrtho(30, .2f));
	}
}
