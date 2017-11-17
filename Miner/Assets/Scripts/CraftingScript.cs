using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingScript : MonoBehaviour
{
	[HideInInspector]
	public float currTime, time;
	public System.DateTime lastAccessed;
	TypeAmount[] amounts;
	[HideInInspector]
	public int index;
	[SerializeField]
	RectTransform bar;
	[SerializeField]
	UnityEngine.UI.Text txt;

	// Update is called once per frame
	void FixedUpdate()
	{
		if (currTime >= time)
		{
			GainResource(1);
			currTime = 0f;
			MCScript.mcScript.UpdateCraftPrices(Positioner().parent);
		}
		else
		{
			currTime += Time.fixedDeltaTime;
			bar.sizeDelta = new Vector2(567f * currTime / time, bar.sizeDelta.y);
			txt.text = currTime.ToString("F1") + " / " + time.ToString();
		}
	}

	Transform Positioner()
	{
		return txt.transform.parent.parent;
	}

	public bool GainResource(int _numberOfTimes)
	{
		int i = -1; while (++i != _numberOfTimes)
		{
			foreach (TypeAmount typeAmount in amounts)
			{
				if (MCScript.SavedAboveData.collectibles[typeAmount.GetIndex()] < (int)typeAmount.GetAmount())
				{
					gameObject.SetActive(false);
					return false;
				}
			}
			foreach (TypeAmount typeAmount in amounts)
			{
				MCScript.IncreaseResource((int)typeAmount.GetIndex(), -(int)typeAmount.GetAmount());
			}
			MCScript.IncreaseResource(index);
		}
		return true;
		//MCScript.SetText(((MCScript.COLLECTIBLES)index).ToString() + " +" + _numberOfTimes, Color.green, Camera.main.ViewportToScreenPoint(new Vector3(.5f, .7f)));
	}

	public void SetCraft(Transform _buyButton)
	{
		int lIndex = int.Parse(System.Text.RegularExpressions.Regex.Match(_buyButton.GetChild(0).GetComponent<UnityEngine.UI.Image>().sprite.name, "\\d+").Value);
		if (lIndex == index)
		{
			Unattach();
			return;
		}
		Positioner().gameObject.SetActive(true);
		index = lIndex;
		lastAccessed = System.DateTime.Now;
		StartCraft(_buyButton);
	}

	public void StartCraft(Transform _buyButton)
	{
		time = int.Parse(System.Text.RegularExpressions.Regex.Match(_buyButton.GetChild(3).GetComponent<UnityEngine.UI.Text>().text, "\\d+").Value);
		currTime = 0f;
		Positioner().SetParent(_buyButton.parent, true);
		Positioner().position = _buyButton.position;
		amounts = new TypeAmount[_buyButton.childCount - 4];
		int i = _buyButton.childCount; while (--i != 3)// there are 4 children already before we add the costimages
		{
			Transform costImage = _buyButton.GetChild(i);
			amounts[i - 4] = (new TypeAmount
				(uint.Parse(System.Text.RegularExpressions.Regex.Match(costImage.GetComponent<UnityEngine.UI.Image>().sprite.name, "\\d+").Value)
				, uint.Parse(System.Text.RegularExpressions.Regex.Match(costImage.GetChild(0).GetComponent<UnityEngine.UI.Text>().text, "\\d+").Value)));
		}
		gameObject.SetActive(true);
	}

	private void OnEnable()
	{
		if (index != -1)
		{
			currTime += (float)(System.DateTime.Now - lastAccessed).TotalSeconds;
			int resourcesGained = (int)currTime / (int)time;
			if (GainResource(resourcesGained))
			{
				currTime -= resourcesGained * time;
			}
			else
			{
				currTime = 0f;
				FixedUpdate();
			}
		}
	}

	private void OnDisable()
	{
		lastAccessed = System.DateTime.Now;
	}

	public void Unattach()
	{
		gameObject.SetActive(false);
		Positioner().gameObject.SetActive(false);
		index = -1;
		currTime = time = 0f;
	}
}
