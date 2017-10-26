using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayButtonScript : MonoBehaviour
{
	[SerializeField]
	MCScript.FUNCTIONCALL functionCall;
	virtual protected void Start()
	{
		MCScript.MouseOver += ButtonClicked;
		MCScript.menuPressed += gameObject.SetActive;
	}

	public void ButtonClicked(RayButtonScript _hitObject)
	{
		if (_hitObject == this)
			// may need future protection, but as of right now we can just leave it be...
			ButtonClicked();
	}

	public virtual void ButtonClicked()
	{
		MCScript.currOpenBuilding = transform.parent.parent.GetComponent<BuildingScript>();
		FindObjectOfType<MCScript>().ButtonClicked((int)functionCall);
	}

	public virtual void GoBack()
	{

	}

	private void OnDestroy()
	{
		MCScript.MouseOver -= ButtonClicked;
		MCScript.menuPressed -= gameObject.SetActive;
	}

}
