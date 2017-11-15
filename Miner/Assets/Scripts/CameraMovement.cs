﻿using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour
{

	public enum CAMERA_MODE { Transition = -1, Above, UnderGround, Map, AboveSlide }
	public static CAMERA_MODE cameraMode = 0;
	public static CAMERA_MODE oldMode = 0;

	PlayerScript target;
	Vector2 lastPos, speed;
	const float acc = .93f;
	public float width;
	public GameObject upperMenu;


	void Start()
	{
		target = FindObjectOfType<PlayerScript>();
	}

	void FixedUpdate()
	{

		switch (cameraMode)
		{
			case CAMERA_MODE.AboveSlide:
			case CAMERA_MODE.Above:
				Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 4f, 6f);
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
				if (transform.position.x > Screen.width)
					transform.position = new Vector3(Screen.width * .5f, transform.position.y);
				else if (transform.position.x < 3f)
					transform.position = new Vector3(3f, transform.position.y);
				break;
			case CAMERA_MODE.UnderGround:
				transform.position += (Vector3)(((Vector2)target.transform.position - (Vector2)transform.position) * .04f);
				// turn position into bitfield space
				// x = (int)transform.position / 200; y = 
				// find closest bits on grid
				// for each bit check if its off
				// if its off turn it on and instantiate a tile
				break;
			case CAMERA_MODE.Map:
				Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 4f, 10f);
				if (Input.GetMouseButton(0))
				{
					if (Input.GetMouseButtonDown(0))
					{
						lastPos = Input.mousePosition;
					}
					else
					{
						speed = (Vector2)Input.mousePosition - lastPos;
						transform.position += (Vector3)speed;
						lastPos = Input.mousePosition;
					}
				}
				break;
			default:
				break;
		}
		if (transform.position.x > width)
			transform.position = new Vector3(width, transform.position.y);
		else if (transform.position.x < 3f)
			transform.position = new Vector3(3f, transform.position.y);

	}

	void Update()
	{
		Camera.main.orthographicSize -= Input.GetAxis("WheelZoom") * .03f;
	}

	// called from animator after the transition
	public void Up()
	{
		cameraMode = CAMERA_MODE.Transition;
		oldMode = CAMERA_MODE.Above;
		MCScript.mcScript.GoUp();
	}

	// Used when we are actually going up
	public void GoVisit(float _x)
	{
		target.transform.position = new Vector2(.3f, 2.45f);
		bool goingDown = transform.position.y == 1f;
		if (goingDown)
		{
			target.ChangeStates(PlayerScript.STATE.Normal);
			target.GetComponent<Rigidbody2D>().gravityScale = 0f;
		}
		else
		{
			target.ChangeStates(PlayerScript.STATE.WalkIn);
			upperMenu.SetActive(true);
		}
		GoView(_x, goingDown);
	}

	// used when we are just peeking (not able to select buttons)
	public void GoView(float _x, bool _goingDown)
	{
		cameraMode = CAMERA_MODE.Transition;
		StartCoroutine(MoveToPos(_x, _goingDown));
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

	static public void Win()
	{

	}

	static public void Lose()
	{

	}

	static public void WorldPosToGridPos(Transform _worldMatrix)
	{
		int bit = Mathf.Max(0,((int)_worldMatrix.position.x) / (/*TileWidth*/2) - 4/*width*/)-1;
		int yStartIndex = Mathf.Max(0, ((int)_worldMatrix.position.y) / -2 - 4/*height*/)-1;// dont forget y is going down
		SaveBelowData savedBelow = MCScript.SavedBelowData;
		int yEndIndex = Mathf.Min(savedBelow.tiles.Length, yStartIndex +11/*height*2 + 1*/);
		int y;
		Vector2 gridPos;
		int x = Mathf.Min(64, bit + 9/*width*2+1*/); while (--x != bit)
		{
			y = yStartIndex; while (++y != yEndIndex)
			{
				if ((savedBelow.tiles[y]&(0x8000000000000000u >> x)) ==0)
				{
					savedBelow.tiles[y] |= (0x8000000000000000u >> x);
					gridPos.x = 2u * x;// 2 is the size of the objects (200 pixles, and the world is set to 100 pixels per unit)
					gridPos.y = -2u * y;
					MCScript.CreateTile(gridPos, y);
				}
			}
		}
	}

	public IEnumerator MoveToPos(float _x, bool _goingDown)
	{
		Vector2 pos = new Vector2(_x, 1f);
		while (Vector2.SqrMagnitude(pos - (Vector2)transform.position) > .4f)
		{
			transform.position = Vector2.LerpUnclamped(transform.position, pos, .1f);
			yield return null;
		}
		transform.position = pos;
		if (_goingDown)
		{
			target.GetComponent<Rigidbody2D>().gravityScale = 1f;
		}
		GetComponent<Animator>().SetBool("Up", !_goingDown);
		yield break;
	}
}
