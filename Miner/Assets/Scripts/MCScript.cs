using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MCScript : MonoBehaviour
{
	const string saveName = "/DontTouch.dat";
	const string saveName2 = "/DontTouch2.dat";
	public const uint differentItemCount = 6;
	public enum SAVEKEY
	{
		MusicVolume, SfxVolume, Speed, Level,
		Total
	}
	public enum MUSIC_CLIP { None = -1, DayTime, NightTime, Start, Boss, Victory }

	public enum TILE_VALUES
	{
		Dirt, Stone, Oil, Artifact, Map, Treasure, Coal, Copper, Nickel, Zinc, Iron, Tin, Aluminum, Opal, Silver, Gold, Ruby, Emerald
			, Azurite, Amythyst, Onyx, Pearl, Chromium, Sapphire, Topaz, Titanium, Platinum, Kryptonium, Plutonium, Marsinium,

		Total
	}
	public enum COLLECTIBLES
	{
		graphite,
		Total
	}
	public enum ITEMS { Beam, Bridge, Marker, MapPiece, Teleporter }
	public enum TILE_STATES { Hidden, Visited, Dug, Placed }
	public enum TILE_GEMTYPES { Single, Mineral, Rhombus, Dagger, Diamond, Heart, Hexagon, Leaf, Triangle }

	static public Text txt;
	[SerializeField]
	Text goldTxt;
	static public MUSIC_CLIP currClip = MUSIC_CLIP.DayTime;
	[HideInInspector]
	public AudioClip[] music_clips;
	public static AudioClip[] audioAttacks, audioDeaths;
	public static float dayTimer = 128f;
	public static AudioSource musicPlayer, sfxPlayer, transition;
	//delete when we have a stable size
	public static uint maxWidth = 240u, maxHeight = 30u;
	//public static uint[,] bitField;
	public static bool day = true;

	enum TRANSITION
	{
		None,
		Fade
	}
	[SerializeField]
	TRANSITION trans;
	AsyncOperation aOp;
	[SerializeField]
	RawImage fader;
	[SerializeField]
	Transform levelSelect;
	static public Transform menu;
	int tempLevel;
	public enum FUNCTIONCALL { Collect, UpgradeMenu, ShopMenu, Upgrade, Sell, Boost, Efficient }
	static public GameObject loadingScreen;
	public GameObject backBtn;
	public LayerMask buttons;
	public delegate void RayHit(RayButtonScript _hitObject);
	public static RayHit MouseOver;
	public delegate void MenuOpened(bool _show);
	public static MenuOpened menuPressed;
	RayButtonScript currPressedBtn, currHeldBtn;
	public AudioClip click;
	int indexOfSell;
	static public BuildingScript currOpenBuilding;
	static public SaveMoneyData savedData;
	static public MCScript mcScript;
	static public CameraMovement cam;

	void Awake()
	{
		if (levelSelect)
		{
			levelSelect.transform.GetChild(1).GetComponent<Button>().interactable = false;
			Texture[] levelimages = Resources.LoadAll<Texture>("LevelImages");
			levelSelect.GetChild(2).GetChild(2).GetComponent<RawImage>().texture = levelimages[UnityEngine.Random.Range(0, levelimages.Length)];
			levelSelect.GetChild(2).GetChild(0).GetComponent<Text>().text = "Level: " + tempLevel;
			if (tempLevel == 0)
				levelSelect.GetChild(0).GetComponent<Button>().interactable = false;
		}
		menu = GameObject.Find("Menus").transform;
		txt = GameObject.Find("AmountTxt").GetComponent<Text>();
		audioAttacks = Resources.LoadAll<AudioClip>("Attacks");
		audioDeaths = Resources.LoadAll<AudioClip>("Deaths");
		music_clips = Resources.LoadAll<AudioClip>("Music");
		DontDestroyOnLoad(gameObject);
		musicPlayer = GetComponents<AudioSource>()[0];
		sfxPlayer = GetComponents<AudioSource>()[1];
		transition = GetComponents<AudioSource>()[2];
		musicPlayer.ignoreListenerVolume = true;
		transition.ignoreListenerVolume = true;
		musicPlayer.PlayOneShot(music_clips[(uint)currClip]);
		//UnityEngine.SceneManagement.SceneManager.LoadScene ( "MainMenu" );
		mcScript = this;
		cam = FindObjectOfType<CameraMovement>();
		LoadStage();
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit2D hitInfo = Physics2D.Raycast(ray.origin, ray.direction, 1000f, buttons);
			if (!hitInfo)
			{
				currHeldBtn = null;
			}
			else
			{
				currHeldBtn = hitInfo.collider.GetComponent<RayButtonScript>();
				MCScript.PlaySoundEffect(click);
			}
		}
		else if (Input.GetMouseButtonUp(0))
		{
			if (currHeldBtn)
			{
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit2D hitInfo = Physics2D.Raycast(ray.origin, ray.direction, 1000f, buttons);
				if (hitInfo)
				{
					RayButtonScript releasedBtn = hitInfo.collider.GetComponent<RayButtonScript>();
					if (currHeldBtn == releasedBtn)
					{
						currHeldBtn.ButtonClicked();
						currPressedBtn = currHeldBtn;
						return;
					}
				}
			}
			if (currPressedBtn)
			{
				currPressedBtn.GoBack();
				currPressedBtn = null;
			}
		}
	}

	private void FixedUpdate()
	{
		if (currOpenBuilding)
		{
			//effCostTxt.text = (.1f/*math needed*/).ToString("F2");
			//effTxt.text = currOpenBuilding.efficiency.ToString("F2");
			//effNextTxt.text = currOpenBuilding.efficiency.ToString("F2") + "->" + (currOpenBuilding.efficiency + .1f/*math needed*/).ToString("F2");
			//upgradeCostTxt.text = currOpenBuilding.collection.ToString("F2");
			//upgradeNextTxt.text = currOpenBuilding.type.ToString("F2");
		}
		dayTimer += Time.deltaTime;
		if (dayTimer >= 150f)
		{
			dayTimer = 0f;
			GameObject.Find("bgSky1").GetComponent<Animator>().SetBool("Transition", day);
			GameObject.Find("outerSpace").GetComponent<Animator>().SetBool("Transition", day);
			day = !day;
			FadeMusic(day ? MUSIC_CLIP.DayTime : MUSIC_CLIP.NightTime);
			// recalculate mineral values
		}
	}

	void LoadScreen(string _sceneName)
	{
		Time.timeScale = 1;
		if (menu)
			menu.gameObject.SetActive(false);
		if (trans == TRANSITION.Fade)
			fader.CrossFadeAlpha(1f, 2f, true);
		if (loadingScreen)
		{
			loadingScreen.SetActive(true);
			StartCoroutine(LoadLevelWithProgressBar(_sceneName));
		}
		else
			SceneManager.LoadSceneAsync(_sceneName);
	}

	public void SetMusicVol(float _vol)
	{
		musicPlayer.volume = _vol;
		SetSaveKey(SAVEKEY.MusicVolume, _vol);
	}

	public void SetSFXVol(float _vol)
	{
		sfxPlayer.volume = _vol;
		SetSaveKey(SAVEKEY.SfxVolume, _vol);
	}

	public void ChangeScene(string _sceneName)
	{
		if (_sceneName == "")
			_sceneName = "Level" + tempLevel;
		LoadScreen(_sceneName);
	}

	public void ChangeScene(int _level)
	{
		LoadScreen("Level" + _level);
	}

	public void ChangeScene()
	{
		LoadScreen("Level" + tempLevel);
	}

	public void IncLevel(int _dir)
	{
		tempLevel += _dir;
		Transform left = levelSelect.GetChild(0), right = levelSelect.GetChild(1), start = levelSelect.GetChild(2);

		if (_dir > 0)
		{
			levelSelect.GetComponent<Animator>().SetTrigger("Forward");
			right.GetComponent<RawImage>().texture = start.GetChild(2).GetComponent<RawImage>().texture;
			left.GetComponent<Button>().interactable = true;
			if (tempLevel >= PlayerPrefs.GetInt(SAVEKEY.Level.ToString()))
			{
				right.GetComponent<Button>().interactable = false;
				tempLevel = PlayerPrefs.GetInt(SAVEKEY.Level.ToString());
			}
		}
		else
		{
			levelSelect.GetComponent<Animator>().SetTrigger("Backward");
			left.GetComponent<RawImage>().texture = start.GetChild(2).GetComponent<RawImage>().texture;
			right.GetComponent<Button>().interactable = true;
			if (tempLevel <= 0)
			{
				left.GetComponent<Button>().interactable = false;
				tempLevel = 0;
			}
		}
		Texture[] levelimages = Resources.LoadAll<Texture>("LevelImages");
		start.GetChild(2).GetComponent<RawImage>().texture = levelimages[UnityEngine.Random.Range(0, levelimages.Length)];
		start.GetChild(0).GetComponent<Text>().text = "Level: " + tempLevel;


	}

	public void ChangeMenu(GameObject _currMenu)
	{
		Animator[] allMenus = menu.GetComponentsInChildren<Animator>();
		foreach (Animator anim in allMenus)
		{
			anim.gameObject.SetActive(false);
		}
		if (_currMenu)
		{
			_currMenu.SetActive(true);
		}
	}

	IEnumerator LoadLevelWithProgressBar(string _sceneName)
	{
		yield return new WaitForSeconds(1);//avoids loading conflicts

		aOp = SceneManager.LoadSceneAsync(_sceneName);

		aOp.allowSceneActivation = false;
		Slider loadingProgBar = null;
		if (loadingScreen)
			loadingProgBar = loadingScreen.transform.GetChild(0).GetComponent<Slider>();
		while (!aOp.isDone)
		{
			if (loadingProgBar)
				loadingProgBar.value = aOp.progress;

			if (aOp.progress == 0.9f)
			{
				if (loadingProgBar)
					loadingProgBar.value = 1.0f;
				aOp.allowSceneActivation = true;
			}

			yield return null;
		}

	}

	public static void Win()
	{

	}

	public void GoBack()
	{
		menuPressed(true);
		currOpenBuilding = null;
	}

	public void SetMainMenu(int _type)
	{
		Transform mainMenu = menu.Find("MainMenu");
		Transform bagContent = mainMenu.Find("Scroll View").GetChild(0).GetChild(0);
		if (bagContent.childCount != 0)
		{
			Destroy(bagContent.GetChild(0).gameObject);
		}
		RectTransform content = (RectTransform)(new GameObject("ContentObject", typeof(RectTransform))).transform;
		content.transform.SetParent(bagContent, false);
		content.pivot = Vector2.up;
		content.anchorMin = Vector2.up;
		content.anchorMax = Vector2.up;
		content.anchoredPosition = Vector2.zero;
		Sprite[] collectibles = Resources.LoadAll<Sprite>("Collectibles");
		GameObject mineralDisplay = Resources.Load<GameObject>("MineralDisplay");
		if (_type == 3)// if we are setting the bag
		{
			// turn off the rest of the menu
			mainMenu.GetChild(6).gameObject.SetActive(false);
			mainMenu.GetChild(2).gameObject.SetActive(false);
			mainMenu.GetChild(3).gameObject.SetActive(false);
			mainMenu.GetChild(4).gameObject.SetActive(false);
			mainMenu.GetChild(5).gameObject.SetActive(false);
			// load the bag in order they were received
			// this helps finding the item the user just got bc it will be at the end of the list
			int i = -1; while (++i != PlayerScript.bag.Count)
			{
				int mineralIndex = (int)PlayerScript.bag[i].GetIndex();
				Transform newItem = Instantiate(mineralDisplay, content).transform;
				Sprite newImage = collectibles[mineralIndex];
				newItem.GetChild(0).GetChild(0).GetComponent<Image>().sprite = newImage;
				newItem.GetChild(1).GetComponent<Text>().text = PlayerScript.bag[i].GetAmount().ToString();
				newItem.GetChild(2).GetComponent<Text>().text = System.Text.RegularExpressions.Regex.Match(newImage.name, "\\D+").Value;
				((RectTransform)newItem).anchoredPosition = new Vector3(2.4f + (i % 3) * 194, -8 - 304 * (i / 3));
			}
			mainMenu.GetChild(7).gameObject.SetActive(i == 0 ? true : false);
		}
		else
		{
			// turn on the rest of the menu
			mainMenu.GetChild(6).gameObject.SetActive(true);
			mainMenu.GetChild(2).gameObject.SetActive(true);
			mainMenu.GetChild(3).gameObject.SetActive(true);
			mainMenu.GetChild(4).gameObject.SetActive(true);
			mainMenu.GetChild(5).gameObject.SetActive(true);
			mainMenu.GetChild(7).gameObject.SetActive(false);
			int finish = 32 + 32 * _type;
			int i = 32 * _type - 1; while (++i != finish)
			{
				Transform newItem = Instantiate(mineralDisplay, content).transform;
				Sprite newImage = collectibles[i];
				if (savedData.minerals[i] == -1)
				{
					newItem.GetChild(0).GetChild(0).gameObject.SetActive(false);
					newItem.GetChild(0).GetChild(1).gameObject.SetActive(true);
					newItem.GetChild(1).gameObject.SetActive(false);
					newItem.GetChild(2).gameObject.SetActive(false);
				}
				else
				{
					Button.ButtonClickedEvent newEvent = newItem.GetComponent<Button>().onClick;
					int passedByRef = i;
					newEvent.AddListener(() => SellMineralMenu(passedByRef));
					newEvent.AddListener(() => SetSellImage(newItem));
					newItem.GetChild(0).GetChild(0).GetComponent<Image>().sprite = newImage;
					newItem.GetChild(1).GetComponent<Text>().text = savedData.minerals[i].ToString();
					newItem.GetChild(2).GetComponent<Text>().text = System.Text.RegularExpressions.Regex.Match(newImage.name, "\\D+").Value;

				}
				((RectTransform)newItem).anchoredPosition = new Vector3(2.4f + ((i - 32 * _type) % 3) * 194, -8 - 304 * ((i - 32 * _type) / 3));
			}
		}
	}

	static public void GoUp()
	{
		savedData = GetSavedData();
		SaveStage();
		GameObject bagBtn = GameObject.Find("MineralCountTxt");
		bagBtn.GetComponent<Text>().text = "0";
		cam.SetCameraMode(-1);
		List<RandomTravelScript> sparks = new List<RandomTravelScript>();
		List<uint> indices = new List<uint>();
		foreach (TypeAmount item in PlayerScript.bag)
		{
			uint index = item.GetIndex();
			indices.Add(index);
			if (savedData.minerals[index] == -1)
			{
				// TODO:
				// new Animation
			}
			sparks.Add( Instantiate(Resources.Load<GameObject>("NewItemParticle"),Camera.main.ScreenToWorldPoint(bagBtn.transform.position),Quaternion.identity).GetComponent<RandomTravelScript>());
			savedData.minerals[index] += (int)item.GetAmount();
		}
		int i = -1; while (++i != PlayerScript.items.Length)
		{
			savedData.items[i] = PlayerScript.items[i];
		}
		PlayerScript.bag = new List<TypeAmount>();
		Transform mainMenu = menu.Find("MainMenu");
		mainMenu.gameObject.SetActive(true);
		mcScript.SetMainMenu(0);
		Transform content = mainMenu.Find("Scroll View").GetChild(0).GetChild(0).GetChild(0);
		i = -1; while (++i!=indices.Count)
		{
			sparks[i].aim = Camera.main.ScreenToWorldPoint(content.GetChild((int)indices[i]).position);
		}
		SaveData(savedData);
	}

	public void GoDown()
	{
		SaveData(savedData);
		savedData = null;
	}

	public void SellMineralMenu(int _index)
	{
		Transform sellMenu = menu.Find("SellMenu");
		sellMenu.gameObject.SetActive(true);
		indexOfSell = _index;
		sellMenu.Find("ValueTxt").GetComponent<Text>().text = "Today's Value: " + (indexOfSell < 0 ? savedData.itemCosts[Mathf.Abs(indexOfSell+1)].ToString("F2") : savedData.dailyMineralValues[indexOfSell].ToString("F2"));

		Slider slider = sellMenu.Find("Slider").GetComponent<Slider>();
		ChangeTabColor(sellMenu.Find("Tabs").GetChild(0));
		sellMenu.Find("SellBtn").gameObject.SetActive(true);
		sellMenu.Find("BuyBtn").gameObject.SetActive(false);
		SetSliderMax();
	}
	// ^^^^^ these functions are called together ^^^^^^^ (needed 2 parameters)
	public void SetSellImage(Transform _parent)
	{
		Transform sellMenu = menu.Find("SellMenu");
		sellMenu.transform.Find("NameTxt").GetComponent<Text>().text = _parent.Find("Name").GetComponent<Text>().text;
		sellMenu.transform.Find("Slider").GetChild(2).GetChild(0).GetChild(0).GetComponent<Image>().sprite = _parent.GetChild(0).GetChild(0).GetComponent<Image>().sprite;
	}

	public void SetSellValue(float _value)
	{
		Transform sellMenu = menu.Find("SellMenu");
		Slider slider = sellMenu.Find("Slider").GetComponent<Slider>();
		slider.value = _value;
		Text goldValTxt = sellMenu.Find("GoldTxt").GetComponent<Text>();
		sellMenu.Find("InputField").Find("Placeholder").GetComponent<Text>().text = slider.value.ToString();
		if (sellMenu.Find("SellBtn").gameObject.activeSelf)
		{
			goldValTxt.text = "Gold:\n+";
		}
		else
		{
			goldValTxt.text = "Gold:\n-";
		}
		goldValTxt.text += slider.value * (indexOfSell < 0 ? savedData.itemCosts[Mathf.Abs(indexOfSell+1)] : savedData.dailyMineralValues[indexOfSell]);
	}

	public void SetSellValueWKeys(string _input)
	{
		Transform sellMenu = menu.Find("SellMenu");
		sellMenu.Find("Slider").GetComponent<Slider>().value = int.Parse(System.Text.RegularExpressions.Regex.Match(_input, "\\d+").Value);
	}

	public void SetSliderMax()
	{
		Transform sellMenu = menu.Find("SellMenu");
		Slider slider = sellMenu.Find("Slider").GetComponent<Slider>();

		if (sellMenu.Find("SellBtn").gameObject.activeSelf)
		{
			slider.maxValue = (indexOfSell < 0 ? savedData.items[Mathf.Abs(indexOfSell+1)] : savedData.minerals[indexOfSell]);
		}
		else
		{
			slider.maxValue = (indexOfSell < 0 ? (int)savedData.gold / savedData.itemCosts[Mathf.Abs(indexOfSell + 1)] : (int)savedData.gold / (int)savedData.dailyMineralValues[indexOfSell]);
		}
		sellMenu.Find("TotalTxt").GetComponent<Text>().text = "Total:\n" + slider.maxValue;
		SetSellValue(slider.value);
	}

	public void SellMineral(bool _selling)
	{
		Transform sellMenu = menu.Find("SellMenu");
		int value = (int)sellMenu.Find("Slider").GetComponent<Slider>().value * (_selling ? -1 : 1);
		if (indexOfSell<0)
		{
			savedData.items[Mathf.Abs(indexOfSell + 1)] += (ushort)value;
			savedData.gold -= value * savedData.itemCosts[Mathf.Abs(indexOfSell + 1)];
		}
		else
		{
			savedData.minerals[indexOfSell] += value;
			savedData.gold -= value * savedData.dailyMineralValues[indexOfSell];
		}
		SetSliderMax();
		goldTxt.text = savedData.gold.ToString("F2");
		// don't save here to allow for mistakes (they can restart and take it back)
		//SaveData(savedData);
	}

	public void ChangeTabColor(Transform _tab)
	{
		Color faded = new Color32(165, 165, 165, 255);
		foreach (Image child in _tab.parent.GetComponentsInChildren<Image>())
		{
			if (child.transform == _tab)
			{
				child.color = Color.white;
			}
			else
			{
				child.color = faded;
			}
		}

	}

	public void ButtonClicked(int _function)
	{
		PlaySoundEffect(click);
		switch ((FUNCTIONCALL)_function)
		{
			case FUNCTIONCALL.Collect:
				savedData.gold += currOpenBuilding.collection;
				txt.transform.position = Camera.main.WorldToScreenPoint(transform.position);
				txt.color = Color.yellow;
				currOpenBuilding.collection = 0f;
				currOpenBuilding = null;
				break;
			case FUNCTIONCALL.UpgradeMenu:
				menu.Find("Upgrade_Menu").gameObject.SetActive(true);
				cam.SetCameraMode((int)CameraMovement.CAMERA_MODE.Transition);
				menuPressed(false);
				// todo :
				// turn interactable = false if we cant buy it
				break;
			case FUNCTIONCALL.ShopMenu:
				menu.Find("Shop").gameObject.SetActive(true);
				cam.SetCameraMode((int)CameraMovement.CAMERA_MODE.Transition);
				menuPressed(false);
				// todo :
				// turn interactable = false if we cant buy it
				break;
			case FUNCTIONCALL.Upgrade:
				break;
			case FUNCTIONCALL.Sell:
				break;
			case FUNCTIONCALL.Boost:
				break;
			case FUNCTIONCALL.Efficient:
				break;
			default:
				break;
		}
	}

	public void Quit()
	{
		Application.Quit();
	}

	//@since depth is a negative number bc as you go down y decreases
	public static int FullHpForDepth(float _y)
	{
		return Mathf.Min(Mathf.RoundToInt(Map(-_y, 0f, 900f, 4f, 28f)), 28);
	}

	static public float Map(float value, float minlow, float minHigh, float maxLow, float maxHigh)
	{

		return (maxLow + (maxHigh - maxLow) * (value - minlow) / (minHigh - minlow));
	}

	public static float XYDistanceSqd(Vector2 _direction)
	{
		return Mathf.Pow(_direction.x, 2f) + Mathf.Pow(_direction.y, 2f);
	}

	static public void SaveStage()
	{
		BinaryFormatter bf = new BinaryFormatter();
		FileStream fs = File.Create(Application.persistentDataPath + saveName);
		SaveData saveData = new SaveData();// creates the information based on what is currently in the scene
		bf.Serialize(fs, saveData);
		fs.Close();
	}

	static public void SaveData(SaveMoneyData _data)
	{
		BinaryFormatter bf = new BinaryFormatter();
		FileStream fs = File.Create(Application.persistentDataPath + saveName2);
		bf.Serialize(fs, _data);
		fs.Close();
	}

	static public SaveMoneyData GetSavedData()
	{
		BinaryFormatter bf = new BinaryFormatter();
		FileStream fs = File.Open(Application.persistentDataPath + saveName2, FileMode.Open);
		SaveMoneyData saved = (SaveMoneyData)bf.Deserialize(fs);
		fs.Close();
		return saved;
	}

	static public void LoadNew()
	{
		// initialize to -1 s0 we know we dont have it and haven't unlocked it
		Vector2 gridPos;
		GameObject cloneTile = Resources.Load<GameObject>("Tile");
		uint x = uint.MaxValue; while (++x != maxWidth)
		{
			uint y = x == 0u ? 0u : uint.MaxValue;
			while (++y != maxHeight)
			{
				gridPos.x = 2u * x;// 2 is the size of the objects (200 pixles, and the world is set to 100 pixels per unit)
				gridPos.y = -2u * y;
				Tile newTile = Instantiate(cloneTile, gridPos, Quaternion.AngleAxis(90 * UnityEngine.Random.Range(0, 4), Vector3.forward)).GetComponent<Tile>();
				newTile.hp = (sbyte)FullHpForDepth(gridPos.y);
				int chance = UnityEngine.Random.Range(0, 500);
				if (chance < 330)
					newTile.value = (byte)TILE_VALUES.Dirt;
				else if (chance < 337)
					newTile.value = (byte)TILE_VALUES.Stone;
				else if (chance < 340)
					newTile.value = (byte)TILE_VALUES.Oil;
				else if (chance < 345)
					newTile.value = (byte)TILE_VALUES.Map;
				else if (chance == 345)
					newTile.value = (byte)TILE_VALUES.Treasure;
				else
					newTile.value = (byte)Map(Mathf.Min(1000f, (y * .5f + 2f) * Mathf.Log((chance - 345), 1.3f)), 0f, 1000f, (float)TILE_VALUES.Coal, (float)(TILE_VALUES.Total - 1));
			}
		}
		SaveStage();
		savedData = new SaveMoneyData();
		SaveData(savedData);
	}

	static public void LoadStage()
	{
		if (File.Exists(Application.persistentDataPath + saveName))
		{
			BinaryFormatter bf = new BinaryFormatter();
			FileStream fs = File.Open(Application.persistentDataPath + saveName, FileMode.Open);
			try
			{
				SaveData saveData = (SaveData)bf.Deserialize(fs);
				fs.Close();
				fs = File.Open(Application.persistentDataPath + saveName2, FileMode.Open);
				savedData = (SaveMoneyData)bf.Deserialize(fs);
				fs.Close();
				//bitField = saveData.bitField;
				Vector2 gridPos;
				GameObject cloneTile = Resources.Load<GameObject>("Tile");
				const int minV = -1;
				int x = saveData.saveTiles.Length; while (--x != minV)
				{
					SaveData.SaveTile saveTile = saveData.saveTiles[x];
					gridPos.x = saveTile.x;
					gridPos.y = saveTile.y;
					Tile newTile = Instantiate(cloneTile, gridPos, Quaternion.AngleAxis(90 * UnityEngine.Random.Range(0, 4), Vector3.forward)).GetComponent<Tile>();
					newTile.value = saveTile.value;
					newTile.hp = (sbyte)FullHpForDepth(gridPos.y);
					newTile.state = saveTile.state;

					switch ((TILE_STATES)newTile.state)
					{
						case TILE_STATES.Visited:
							newTile.Initialized();
							break;
						case TILE_STATES.Dug:
						case TILE_STATES.Placed:
							Destroy(newTile.GetComponent<Collider2D>());
							break;
						default:
							break;
					}
				}
				cloneTile = Resources.Load<GameObject>("Building");
				gridPos.y = 2.5f;
				x = saveData.saveBuildings.Length; while (--x != minV)
				{
					SaveData.SaveBuilding saveBuilding = saveData.saveBuildings[x];
					gridPos.x = saveBuilding.xPos;
					BuildingScript newBuilding = Instantiate(cloneTile, gridPos, Quaternion.identity).GetComponent<BuildingScript>();
					newBuilding.type = saveBuilding.buildingType;
					newBuilding.efficiency = saveBuilding.efficiency;
					newBuilding.efficiency -= .0000001f * (float)(System.DateTime.Now - savedData.currTime).TotalSeconds / Time.fixedDeltaTime;// the .0001 is from building scripts update.
					if (newBuilding.efficiency < .5f)
					{
						newBuilding.efficiency = .5f;
					}
				}
				mcScript.goldTxt.text = savedData.gold.ToString("F2");
				// TODO:
				// update timers
				//
			}
			catch (Exception)
			{
				fs.Close();
				LoadNew();
			}
		}
		else
		{
			LoadNew();
		}
	}

	public void ResetKeys()
	{
		uint i = uint.MaxValue;
		while (++i != (uint)SAVEKEY.Total)
			PlayerPrefs.SetFloat(System.Enum.GetName(typeof(SAVEKEY), i), 0f);
	}

	static public void SetSaveKey(SAVEKEY _saveKey, float _val)
	{
		PlayerPrefs.SetFloat(_saveKey.ToString(), _val);
	}

	public void SwitchMusic(MUSIC_CLIP _newclip)
	{
		if (_newclip == currClip)
		{
			return;
		}
		musicPlayer.Stop();
		currClip = _newclip;
		musicPlayer.PlayOneShot(music_clips[(uint)currClip]);
	}

	public void FadeMusic(MUSIC_CLIP _newclip)
	{
		if (_newclip == currClip)
		{
			return;
		}
		currClip = _newclip;
		if (musicPlayer.isPlaying)
		{
			transition.PlayOneShot(music_clips[(uint)_newclip]);
			StartCoroutine(FadeInAndOut(transition, musicPlayer));
		}
		else
		{
			musicPlayer.PlayOneShot(music_clips[(uint)_newclip]);
			StartCoroutine(FadeInAndOut(musicPlayer, transition));
		}
	}

	static public void PlaySoundEffect(AudioClip _clip)
	{
		sfxPlayer.PlayOneShot(_clip);
	}

	static public void PlayRandomAttack()
	{
		sfxPlayer.PlayOneShot(audioAttacks[UnityEngine.Random.Range(0, audioAttacks.Length)]);
	}

	static public void PlayRandomDeath()
	{
		sfxPlayer.PlayOneShot(audioDeaths[UnityEngine.Random.Range(0, audioDeaths.Length)]);
	}

	IEnumerator FadeInAndOut(AudioSource _in, AudioSource _out)
	{
		const float transSpeed = .003f;
		while (_in.volume < .985f)
		{
			_in.volume += transSpeed;
			_out.volume -= transSpeed;
			yield return null;
		}
		_in.volume = 1f;
		_out.volume = 0f;
		_out.Stop();
		yield break;
	}
}

public struct TypeAmount
{
	// the first 7 to the left are which index (0-95 (127 total))
	// the rest are the amount (max of 33,554,431)
	uint value;
	public TypeAmount(uint _value)
	{
		value = _value;
	}
	public TypeAmount(MCScript.TILE_VALUES _value)
	{
		value = ((uint)(_value - MCScript.TILE_VALUES.Coal)) << 25;
	}
	public TypeAmount(MCScript.COLLECTIBLES _value)
	{
		value = ((uint)_value) << 25;
	}
	public uint GetIndex()
	{
		return (value & 0xFE000000) >> 25;
	}
	public uint GetAmount()
	{
		// apparently if a hex code starts with 0 it's an int, otherwise it's a uint.
		return (value & 0x01FFFFFFu);
	}
	public void SetAmount(uint _value)
	{
		value |= _value;
	}
	public static TypeAmount operator +(TypeAmount first, uint second)
	{
		// no bit manipulation required bc the index is on the left. (if values start going above max, we will need to do something different)
		return new TypeAmount(first.value + second);

	}
	public static implicit operator TypeAmount(uint value)
	{
		return new TypeAmount(value);
	}
	public static implicit operator TypeAmount(MCScript.TILE_VALUES value)
	{
		return new TypeAmount(value);
	}
	static public bool operator ==(TypeAmount _first, TypeAmount _second)
	{
		return (_first.value & 0xFE000000) == (_second.value & 0xFE000000);
	}
	static public bool operator !=(TypeAmount _first, TypeAmount _second)
	{
		return (_first.value & 0xFE000000) != (_second.value & 0xFE000000);
	}
	static public bool operator ==(TypeAmount _first, uint _second)
	{
		return (_first.value & 0xFE000000) == (_second & 0xFE000000);
	}
	static public bool operator !=(TypeAmount _first, uint _second)
	{
		return (_first.value & 0xFE000000) != (_second & 0xFE000000);
	}
	public override bool Equals(object obj)
	{
		return (value & 0xFE000000).Equals(obj);
	}
	public override int GetHashCode()
	{
		return (value & 0xFE000000).GetHashCode();
	}

}

[Serializable]
public class SaveData
{
	[Serializable]
	public struct SaveBuilding
	{
		public float xPos, efficiency;
		public byte buildingType;
		public SaveBuilding(BuildingScript _build)
		{
			xPos = _build.transform.position.x;
			efficiency = _build.efficiency;
			buildingType = _build.type;
		}
	}
	[Serializable]
	public struct SaveTile
	{
		public byte value, state;
		public float x, y;
		public SaveTile(Tile _tile)
		{
			value = _tile.value;
			state = _tile.state;
			x = _tile.transform.position.x;
			y = _tile.transform.position.y;
		}
	}
	//public uint[,] bitField;
	public SaveBuilding[] saveBuildings;
	public SaveTile[] saveTiles;
	public SaveData()
	{
		//if (MCScript.bitField == null)
		//{
		//	bitField = new uint[3,1000];
		//}
		//else
		//{
		//	bitField = MCScript.bitField;
		//}
		Tile[] tileObjects = GameObject.FindObjectsOfType<Tile>();
		saveTiles = new SaveTile[tileObjects.Length];
		int i = tileObjects.Length; while (--i != -1)
		{
			saveTiles[i] = new SaveTile(tileObjects[i]);
		}
		BuildingScript[] buildingObjects = GameObject.FindObjectsOfType<BuildingScript>();
		saveBuildings = new SaveBuilding[buildingObjects.Length];
		i = buildingObjects.Length; while (--i != -1)
		{
			saveBuildings[i] = new SaveBuilding(buildingObjects[i]);
		}
	}
}

[Serializable]
public class SaveMoneyData
{
	public ushort[] itemCosts;
	public ushort[] items;
	public int[] minerals;
	public float[] dailyMineralValues;
	public uint upgrades;// bit field
	public uint bagSize;
	public float gold;
	public System.DateTime currTime;
	public List<int> highsAndLows;
	public SaveMoneyData()
	{
		currTime = System.DateTime.Now;
		upgrades = CoppyScript.upgrade;
		bagSize = PlayerScript.maxBagSize;
		gold = 0f;
		items = new ushort[MCScript.differentItemCount];
		// TODO
		// balance out costs
		itemCosts = new ushort[] { 2, 2, 2, 2, 2, 2 };
		minerals = new int[] {
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 // minerals
			, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 // bars
			, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }; // crafts
		dailyMineralValues = new float[minerals.Length];
		highsAndLows = RandomizeValues();
	}
	public List<int> RandomizeValues()
	{
		const int mineralMax = 32, barMax = 64, craftMax = 96;
		int i = -1; while (++i != mineralMax)
		{
			dailyMineralValues[i] = Mathf.Pow(1f + UnityEngine.Random.Range(0f, .6f), i);
		}
		--i; while (++i != barMax)
		{
			dailyMineralValues[i] = 200f + Mathf.Pow(2.3f + UnityEngine.Random.Range(0f, .6f), i - 31);
		}
		--i; while (++i != craftMax)
		{
			dailyMineralValues[i] = 20f + Mathf.Pow(1.5f + UnityEngine.Random.Range(0f, .6f), i - 61);
		}

		int highOutlier = UnityEngine.Random.Range(0, mineralMax), lowOutlier;
		List<int> selectedHighLow = new List<int>(6);

		// minerals
		do
		{
			lowOutlier = UnityEngine.Random.Range(0, mineralMax);
		} while (lowOutlier != highOutlier);
		selectedHighLow.Add(highOutlier); selectedHighLow.Add(lowOutlier);
		dailyMineralValues[highOutlier] *= UnityEngine.Random.Range(1.2f, 2.3f);
		dailyMineralValues[lowOutlier] *= UnityEngine.Random.Range(.3f, .8f);

		//bars
		highOutlier = UnityEngine.Random.Range(0, barMax);
		do
		{
			lowOutlier = UnityEngine.Random.Range(0, barMax);
		} while (lowOutlier != highOutlier);
		selectedHighLow.Add(highOutlier); selectedHighLow.Add(lowOutlier);
		dailyMineralValues[highOutlier] *= UnityEngine.Random.Range(1.2f, 2.3f);
		dailyMineralValues[lowOutlier] *= UnityEngine.Random.Range(.3f, .8f);

		// crafts
		highOutlier = UnityEngine.Random.Range(0, craftMax);
		do
		{
			lowOutlier = UnityEngine.Random.Range(0, craftMax);
		} while (lowOutlier != highOutlier);
		selectedHighLow.Add(highOutlier); selectedHighLow.Add(lowOutlier);
		dailyMineralValues[highOutlier] *= UnityEngine.Random.Range(1.2f, 2.3f);
		dailyMineralValues[lowOutlier] *= UnityEngine.Random.Range(.3f, .8f);

		return selectedHighLow;
	}
}