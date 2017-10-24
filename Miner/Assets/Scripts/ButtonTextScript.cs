using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonTextScript : RayButtonScript
{
	// TODO : DELETE THIS (we do not want any memory allocated for all of these goddamn buttons)
	// if we need a better way to postion things uncomment this to get a good posistion
	public enum TEXTDISPLAY { Type, Cost, Upgrade, Efficiency, Collection }
	public TEXTDISPLAY[] displayTypes;
	TextMesh[] txts;


	protected override void Start()
	{
		base.Start();
		TextMesh[] childTxts = GetComponentsInChildren<TextMesh>();
		txts = new TextMesh[displayTypes.Length];
		int i = -1; while (++i !=txts.Length)
		{
			txts[i] = childTxts[i];
		}
	}

	private void FixedUpdate()
	{
		BuildingScript parent = transform.parent.parent.GetComponent<BuildingScript>();
		int i = -1; while (++i != txts.Length)
		{
			switch (displayTypes[i])
			{
				case TEXTDISPLAY.Cost:
					txts[i].text = (.1f/*math needed*/).ToString("F2");
					break;
				case TEXTDISPLAY.Efficiency:
					txts[i].text = parent.efficiency.ToString("F2");
					break;
				case TEXTDISPLAY.Upgrade:
					txts[i].text = parent.efficiency.ToString("F2") + "->" + (parent.efficiency + .1f/*math needed*/).ToString("F2");
					break;
				case TEXTDISPLAY.Collection:
					txts[i].text = parent.collection.ToString("F2");
					break;
				case TEXTDISPLAY.Type:
					txts[i].text = parent.type.ToString("F2");
					break;
				default:
					break;
			}
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		Destroy(transform.parent.gameObject);
	}
}
