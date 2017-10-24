using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingScript : MonoBehaviour
{
	[HideInInspector]
	public float efficiency = 100f, collection;
	[HideInInspector]
	public byte type;

	private void FixedUpdate()
	{
		if (efficiency > .5f)
		{
			efficiency -= .000001f;
		}
		collection += .0001f;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{

		if (CameraMovement.cameraMode == CameraMovement.CAMERA_MODE.Above || CameraMovement.cameraMode == CameraMovement.CAMERA_MODE.AboveSlide)
		{
			Instantiate(Resources.Load<GameObject>("buildingMainMenu" + type), transform, false);
		}
	}
}
