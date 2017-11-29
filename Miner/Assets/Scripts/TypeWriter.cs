using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TypeWriter : MonoBehaviour
{
	public class SmartText
	{
		public string text;
		public System.Object tutStep;
		public AnyFunction function;
		public SmartText(string _text, AnyFunction _func = null, System.Object _step = null)
		{
			function = _func;
			text = _text;
			tutStep = _step;
		}
	}
	List<SmartText> thingsToSay = new List<SmartText>();
	public UnityEngine.UI.RawImage chatBox;// public to allow changing of the image
	public Transform target;// public to allow changing of who to focus
	[SerializeField]
	UnityEngine.UI.Text skipText;
	[HideInInspector]
	public UnityEngine.UI.Text txt;// public to allow changing color
	[SerializeField]
	float typingSpeed = .08f, fadeSpeed = .01f, minSize = 25f, maxSize = 80f;
	[HideInInspector]
	public Vector2 chatBoxOffset;// public in case we change targets, we might need to change the offset as well
	static RectTransform canvas;
	Coroutine currCoroutine;
	bool fade;

	private void Start()
	{
		if (!canvas)
		{//								if this gives you an error, rename your canvas CanvasScreenSpace. to prevent confusion if you have more than 1
			canvas = (RectTransform)GameObject.Find("CanvasScreenSpace").transform;

		}
		if (!chatBox)
		{
			chatBox = Instantiate(Resources.Load<GameObject>("DefaultChatBox"), canvas.Find("Panel")).GetComponent<UnityEngine.UI.RawImage>();
		}
		if (!txt)
		{
			if (chatBox.transform.childCount != 0)
			{
				// in case they forgot to set it
				txt = chatBox.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>();
			}
			else
			{
				// effortless... you pretty much need to set the text before hand
				txt = Instantiate(Resources.Load<GameObject>("DefaultChatText"), chatBox.transform).GetComponent<UnityEngine.UI.Text>();
				Rect bounds = chatBox.rectTransform.rect;
				txt.transform.localScale = Vector3.one;
				txt.transform.localPosition = bounds.center;
				txt.rectTransform.sizeDelta = new Vector2(bounds.size.x * .8f, bounds.size.y * .5f);
			}
		}
		else
		{
			txt.transform.SetParent(chatBox.transform, true);
		}
		if (!target)
		{
			target = transform;
		}

		TurnOff();
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		if (fade)
		{
			Color alphaI = chatBox.color;
			if (alphaI.a > .02f)
			{
				Color alphaT = txt.color;
				Color alphaS = skipText.color;
				alphaI.a -= fadeSpeed;
				alphaT.a = alphaI.a;
				alphaS.a = alphaI.a;
				chatBox.color = alphaI;
				txt.color = alphaT;
				skipText.color = alphaT;
			}
			else
			{
				TurnOff();
			}
		}
		Rect chatBoxRect = chatBox.rectTransform.rect;
		chatBoxRect.width *= chatBox.transform.lossyScale.x;
		chatBoxRect.height *= chatBox.transform.lossyScale.y;
		Vector3 pos = (Vector2)Camera.main.WorldToScreenPoint(target.position) + chatBoxOffset;
		if (chatBoxRect.xMin + pos.x < 0f)
		{
			if (chatBox.transform.localScale.x < 0f)
			{
				chatBox.transform.localScale = new Vector3(-chatBox.transform.localScale.x, chatBox.transform.localScale.y, 1f);
				txt.transform.localScale = new Vector3(-txt.transform.localScale.x, txt.transform.localScale.y, 1f);
				if (skipText)
					skipText.transform.localScale = new Vector3(-skipText.transform.localScale.x, skipText.transform.localScale.y, 1f);
			}
			else
			{
				pos.x = -chatBoxRect.xMin;
			}
		}
		else
		{
			if (chatBoxRect.xMax + pos.x > Screen.width)
			{
				if (chatBox.transform.localScale.x > 0f)
				{
					chatBox.transform.localScale = new Vector3(-chatBox.transform.localScale.x, chatBox.transform.localScale.y, 1f);
					txt.transform.localScale = new Vector3(-txt.transform.localScale.x, txt.transform.localScale.y, 1f);
					if (skipText)
						skipText.transform.localScale = new Vector3(-skipText.transform.localScale.x, skipText.transform.localScale.y, 1f);
				}
				else
				{
					pos.x = Screen.width - chatBoxRect.xMax;
				}
			}
		}

		if (chatBoxRect.yMin + pos.y < 0f)
		{
			pos.y = -chatBoxRect.yMin;
		}
		else if (chatBoxRect.yMax + pos.y > Screen.height)
		{
			pos.y = Screen.height - chatBoxRect.yMax;
		}
		chatBox.transform.position = pos;
	}

	public void TurnOff()
	{
		if (thingsToSay.Count != 0)
		{
			Type();
		}
		else
		{
			currCoroutine = null;
			enabled = false;
			chatBox.gameObject.SetActive(false);
		}
	}

	public void Skip()
	{
		if (currCoroutine != null)
		{// we need to finish the rest of the coroutine
			StopCoroutine(currCoroutine);
			GetComponent<AudioSource>().Stop();
			if (thingsToSay.Count != 0)
			{
				if (thingsToSay[0].function != null)
					thingsToSay[0].function(thingsToSay[0].tutStep);
				thingsToSay.RemoveAt(0);
			}
		}
		TurnOff();
	}

	public void AddMessage(string _text,AnyFunction _func = null, System.Object _tutStep = null)
	{
		thingsToSay.Add(new SmartText(_text, _func, _tutStep));
		if (!enabled)
		{
			Type();
		}
	}

	void Type()
	{
		// yes we do need 3 incase they are all differently set in the editor
		Color alphaI = chatBox.color;
		Color alphaT = txt.color;
		Color alphaS = skipText.color;
		alphaI.a = .85f;
		alphaT.a = 1f;
		alphaS.a = 1f;
		chatBox.color = alphaI;
		txt.color = alphaT;
		skipText.color = alphaT;
		enabled = true;
		chatBox.gameObject.SetActive(true);
		txt.text = string.Empty;
		currCoroutine = StartCoroutine(TypeOut());
		fade = false;
		GetComponent<AudioSource>().Play();
	}

	IEnumerator TypeOut()
	{
		SmartText smartTxt = thingsToSay[0];
		txt.fontSize = (int)MCScript.Map(1f / smartTxt.text.Length, .001f, 1f, minSize, maxSize);
		yield return new WaitForSeconds(.35f);// the length of the typewriter start noise
		foreach (char letter in smartTxt.text)
		{
			txt.text += letter;
			yield return new WaitForSeconds(typingSpeed);
		}
		GetComponent<AudioSource>().Stop();
		yield return new WaitForSeconds(1.6f);
		currCoroutine = null;
		thingsToSay.RemoveAt(0);
		if (smartTxt.function != null)
		{
			smartTxt.function(smartTxt.tutStep);
		}
		fade = true;
		yield break;
	}
}
