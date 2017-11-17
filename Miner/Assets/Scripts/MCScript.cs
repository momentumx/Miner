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
	const string saveStageFile = "/DontTouch.dat";
	const string saveAboveFile = "/DontTouch2.dat";
	const string saveBothFile = "/DontTouch3.dat";
	const string saveBelowFile = "/DontTouch4.dat";
	public const uint differentItemCount = 5;

	public enum TILE_VALUES
	{
		Dirt, Stone, Oil, Artifact, Map, Treasure, Coal, Copper, Nickel, Opal, Zinc, Iron, Tin, Aluminum, Amethyst, Silver, Gold, Ruby, Emerald
			, Azurite, Onyx, Pearl, Chromium, Sapphire, Topaz, Titanium, Diamond, Platinum, Kryptonium, Plutonium, Rubber, Marsinium,

		Total
	}
	public enum COLLECTIBLES
	{
		// minerals
		Coal, Copper, Nickel, Opal, Zinc, Iron, Tin, Aluminum, Amethyst, Silver, Gold, Ruby, Emerald, Azurite, Onyx, Pearl, Chromium, Sapphire
			, Topaz, Titanium, Diamond, Platinum, Kryptonium, Plutonium, Marsinium

			// smeltables
			, CopperBar, NickelBar, ZincBar, IronBar, TinBar, Brass, Bronze, Silicone, AluminumBar, Glass, SilverBar, ChromiumBar, AlloySteel, ActivatedCharcoal
			, StainleeSteel, GoldBar, TitaniumBar, PlatinumBar, Kryptonite, PlutoniumBar, MarsiniumBar

			// craftables
			, Graphite, Wire, Nail, ArmorPlate, HeatSink, Tools, StrongAxe, Magnet, Pipe, Valve, Can, Spring, Gears, GoldAxe, Foil, Turbine, RubyAxe, Knob, Propeller, Zipper, SapphireAxe, Bearings, Battery, DiamondAxe
			, Frame, Fuselage, Transformer, CarbonFilter, Solder, KryptoAxe, MicroChip, Mirror, GPS, UltimateWeapon

			// items
			, Beam, Bridge, Marker, MapPiece, Teleporter

			// unfindables
			, Artifact1,

		Total
	}
	public enum BUILDINGS { OilRig, Jewelry, Market, RadioTower, Terrarium, Telescope, Teleporter }
	public enum TILE_STATES { Hidden, Visited, Dug, Placed }
	public enum TILE_GEMTYPES { Single, Mineral, Rhombus, Dagger, Diamond, Heart, Hexagon, Leaf, Triangle }
	public enum MENUTYPE { Minerals, Bars, Crafts, Bag, Shop, Items, Crafting }
	static public MENUTYPE menuType;
	public enum MISSION { }


	static public Text txt;
	public Text goldTxt;

	public static float dayTimer = 0f;
	//delete when we have a stable size
	public static uint maxWidth = 6u, maxHeight = 8u;
	public static bool day = true;
	static GameObject cloneTile;

	public enum FUNCTIONCALL { Collect, UpgradeMenu, ShopMenu, Upgrade, Sell, Boost, Efficient }
	public LayerMask buttons;
	public delegate void RayHit(RayButtonScript _hitObject);
	public static RayHit MouseOver;
	public delegate void MenuOpened(bool _show);
	public static MenuOpened menuPressed;
	RayButtonScript currPressedBtn, currHeldBtn;
	public AudioClip click;
	int indexOfSell;
	static public Transform menu, mainMenuContent;
	static public BuildingScript currOpenBuilding;
	static public SaveBothGroundData savedBothData;
	static SaveBelowData savedBelowData;
	static SaveAboveData savedAboveData;
	static public MCScript mcScript;
	public CraftingScript smelt, craft;

	void Awake()
	{
		cloneTile = Resources.Load<GameObject>("Tile");
		menu = GameObject.Find("Menus").transform;
		mainMenuContent = menu.Find("MainMenu").Find("Scroll View").GetChild(0).GetChild(0);
		txt = GameObject.Find("AmountTxt").GetComponent<Text>();

		//UnityEngine.SceneManagement.SceneManager.LoadScene ( "MainMenu" );
		mcScript = this;
		PersistentManager.mmScript.SwitchMusic(PersistentManager.MUSIC_CLIP.DayTime);
		PersistentManager.SetAllAudioSources();
		LoadStage();
		TILE_VALUES temp;
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
				PersistentManager.PlaySoundEffect(click);
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

#if UNITY_EDITOR
		//if (Input.GetKeyDown(KeyCode.Space))
		//{
		//	Transform content = menu.Find("MainMenu").Find("Scroll View").GetChild(0).GetChild(0).GetChild(0);
		//	GameObject bagBtn = GameObject.Find("MineralCountTxt");
		//	bagBtn.GetComponent<Text>().text = "0";
		//	List<RandomTravelScript> sparks = new List<RandomTravelScript>();
		//	Vector3 bagPos = bagBtn.transform.position;
		//	bagPos.z = 0f;
		//
		//	RandomTravelScript spark = Instantiate(Resources.Load<GameObject>("NewItemParticle"), bagPos, Quaternion.identity).GetComponent<RandomTravelScript>();
		//	spark.aim = content.GetChild(UnityEngine.Random.Range(0, 4));
		//	spark.offSet = new Vector2(.7f, -.5f);
		//	spark.timer = 35u;
		//}
#endif
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
			PersistentManager.mmScript.FadeMusic(day ? PersistentManager.MUSIC_CLIP.DayTime : PersistentManager.MUSIC_CLIP.NightTime);
			// recalculate mineral values
		}
	}

	public void ChangeMenu(GameObject _currMenu)
	{
		int i = -1; while (++i != menu.childCount)
		{
			menu.GetChild(i).gameObject.SetActive(false);
		}
		if (_currMenu)
		{
			_currMenu.SetActive(true);
		}
	}

	public void ChangeMenuType(int _type)
	{
		menuType = (MENUTYPE)_type;
	}

	public static void Win()
	{

	}

	public void GoBack()
	{
		menuPressed(true);
		currOpenBuilding = null;
	}

	public void SetMainMenu()
	{
		Transform mainMenu = menu.Find("MainMenu");
		if (mainMenuContent.childCount != 0)
		{
			Destroy(mainMenuContent.GetChild(0).gameObject);
		}
		RectTransform content = (RectTransform)(new GameObject("ContentObject", typeof(RectTransform))).transform;
		content.transform.SetParent(mainMenuContent, false);
		content.pivot = Vector2.up;
		content.anchorMin = Vector2.up;
		content.anchorMax = Vector2.up;
		content.anchoredPosition = Vector2.zero;
		Sprite[] collectibles = Resources.LoadAll<Sprite>("Collectibles");
		GameObject mineralDisplay = Resources.Load<GameObject>("MineralDisplay");
		int i, finish, offset;
		// TODO:
		// adapt the menu for each type
		Transform tab = mainMenu.Find("Tabs");

		switch (menuType)
		{
			case MENUTYPE.Minerals:
			case MENUTYPE.Bars:
			case MENUTYPE.Crafts:
				// turn on the rest of the menu
				mainMenu.GetChild(6).gameObject.SetActive(true);
				mainMenu.GetChild(2).gameObject.SetActive(true);
				mainMenu.GetChild(3).gameObject.SetActive(true);
				mainMenu.GetChild(4).gameObject.SetActive(true);
				mainMenu.GetChild(5).gameObject.SetActive(true);
				mainMenu.GetChild(7).gameObject.SetActive(false);
				mainMenu.GetChild(8).gameObject.SetActive(false);
				mainMenu.GetChild(9).gameObject.SetActive(false);
				switch (menuType)
				{
					case MENUTYPE.Bars:
						ChangeTabColor(tab.GetChild(1));
						i = (int)COLLECTIBLES.Marsinium;
						finish = (int)COLLECTIBLES.Graphite;
						break;
					case MENUTYPE.Crafts:
						ChangeTabColor(tab.GetChild(2));
						i = (int)COLLECTIBLES.MarsiniumBar;
						finish = (int)COLLECTIBLES.Beam;
						break;
					default:
						ChangeTabColor(tab.GetChild(0));
						i = -1;
						finish = (int)COLLECTIBLES.CopperBar;
						break;
				}
				offset = i + 1;
				while (++i != finish)
				{
					Transform newItem = Instantiate(mineralDisplay, content).transform;
					if (savedAboveData.collectibles[i] == -1)
					{
						newItem.GetChild(0).GetChild(0).gameObject.SetActive(false);// image
						newItem.GetChild(0).GetChild(1).gameObject.SetActive(true);// quetion mark
					}
					else
					{
						Button.ButtonClickedEvent newEvent = newItem.GetComponent<Button>().onClick;
						int passedByRef = i;
						newEvent.AddListener(() => SellMineralMenu(passedByRef));
						newEvent.AddListener(() => SetSellImage(newItem));
						newItem.GetChild(0).GetChild(0).GetComponent<Image>().sprite = collectibles[i];// image
						newItem.GetChild(1).GetComponent<Text>().text = savedAboveData.collectibles[i].ToString();// count
						newItem.GetChild(2).GetComponent<Text>().text = ((COLLECTIBLES)i).ToString();// name

					}
					((RectTransform)newItem).anchoredPosition = new Vector3(2.4f + ((i - offset) % 3) * 194, -8 - 304 * ((i - offset) / 3));
				}
				break;
			case MENUTYPE.Bag:
				// turn off the rest of the menu
				mainMenu.GetChild(6).gameObject.SetActive(false);
				mainMenu.GetChild(2).gameObject.SetActive(false);
				mainMenu.GetChild(3).gameObject.SetActive(false);
				mainMenu.GetChild(4).gameObject.SetActive(false);
				mainMenu.GetChild(5).gameObject.SetActive(false);
				mainMenu.GetChild(8).gameObject.SetActive(true);// items button
				mainMenu.GetChild(9).gameObject.SetActive(true);// bag tab
																// load the bag in order they were received
																// this helps finding the item the user just got bc it will be at the end of the list
				i = -1; while (++i != PlayerScript.bag.Count)
				{
					int mineralIndex = (int)PlayerScript.bag[i].GetIndex();
					Transform newItem = Instantiate(mineralDisplay, content).transform;
					Sprite newImage = collectibles[mineralIndex];
					newItem.GetChild(0).GetChild(0).GetComponent<Image>().sprite = newImage;
					newItem.GetChild(1).GetComponent<Text>().text = PlayerScript.bag[i].GetAmount().ToString();
					newItem.GetChild(2).GetComponent<Text>().text = ((COLLECTIBLES)mineralIndex).ToString();
					((RectTransform)newItem).anchoredPosition = new Vector3(2.4f + (i % 3) * 194, -8 - 304 * (i / 3));
				}
				mainMenu.GetChild(7).gameObject.SetActive(i == 0 ? true : false);// empty text

				break;
			case MENUTYPE.Shop:
				ChangeTabColor(tab.GetChild(3));
				i = (int)COLLECTIBLES.UltimateWeapon;
				offset = i + 1;
				finish = (int)COLLECTIBLES.Artifact1;
				while (++i != finish)
				{
					Transform newItem = Instantiate(mineralDisplay, content).transform;
					Button.ButtonClickedEvent newEvent = newItem.GetComponent<Button>().onClick;
					int passedByRef = i;
					newEvent.AddListener(() => SellMineralMenu(passedByRef));
					newEvent.AddListener(() => SetSellImage(newItem));
					newItem.GetChild(0).GetChild(0).GetComponent<Image>().sprite = collectibles[i];
					newItem.GetChild(1).GetComponent<Text>().text = savedAboveData.collectibles[i].ToString();
					newItem.GetChild(2).GetComponent<Text>().text = ((COLLECTIBLES)i).ToString();
					((RectTransform)newItem).anchoredPosition = new Vector3(2.4f + ((i - offset) % 3) * 194, -8 - 304 * ((i - offset) / 3));
				}
				break;
			case MENUTYPE.Items:
				i = -1;
				finish = (int)differentItemCount;
				while (++i != finish)
				{
					if (PlayerScript.items[i] != 0)
					{
						Transform newItem = Instantiate(mineralDisplay, content).transform;
						if (CameraMovement.oldMode == CameraMovement.CAMERA_MODE.UnderGround)
						{
							int passedByRef = i;
							newItem.GetComponent<Button>().onClick.AddListener(() => ItemUsed(passedByRef));
						}
						newItem.GetChild(0).GetChild(0).GetComponent<Image>().sprite = collectibles[i + (int)COLLECTIBLES.Beam];
						newItem.GetChild(1).GetComponent<Text>().text = PlayerScript.items[i].ToString();
						newItem.GetChild(2).GetComponent<Text>().text = (i + COLLECTIBLES.Beam).ToString();
						((RectTransform)newItem).anchoredPosition = new Vector3(2.4f + (i % 3) * 194, -8 - 304 * (i / 3));
					}
				}
				break;
			default:
				break;
		}
	}

	public void ItemUsed(int _itemIndex)
	{
		menu.Find("MainMenu").gameObject.SetActive(false);
		--PlayerScript.items[_itemIndex - (int)COLLECTIBLES.Beam];
	}

	public void GoUp()
	{
		int i = -1; while (++i != differentItemCount)
		{
			SavedAboveData.collectibles[i + (int)COLLECTIBLES.Beam] = PlayerScript.items[i];
		}
		GameObject.Find("LowerMenu").SetActive(false);
		// this sets the main menu
		// this also saves the data after it loops through the bag
		// also gets rid of bag data
		StartCoroutine(CreateSparks());
	}

	public void GoDown()
	{
		// make sure the player has the items it might of boughten
		int i = -1; while (++i != differentItemCount)
		{
			PlayerScript.items[i] = (ushort)SavedAboveData.collectibles[i + (int)COLLECTIBLES.Beam];
		}
		SaveAll(savedBothData, SavedAboveData);
		savedAboveData = null;
	}

	public void SellMineralMenu(int _index)
	{
		Transform sellMenu = menu.Find("SellMenu");
		menu.Find("MainMenu").gameObject.SetActive(false);
		sellMenu.gameObject.SetActive(true);
		indexOfSell = _index;
		sellMenu.Find("ValueTxt").GetComponent<Text>().text = "Today's Price: " + savedAboveData.dailyMineralValues[indexOfSell].ToString("F2");

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
		goldValTxt.text += slider.value * savedAboveData.dailyMineralValues[indexOfSell];
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
			slider.maxValue = savedAboveData.collectibles[indexOfSell];
		}
		else
		{
			slider.maxValue = (int)savedBothData.gold / (int)savedAboveData.dailyMineralValues[indexOfSell];
		}
		sellMenu.Find("TotalTxt").GetComponent<Text>().text = "Total:\n" + slider.maxValue;
		SetSellValue(slider.value);
	}

	public void SellMineral(bool _selling)
	{
		Transform sellMenu = menu.Find("SellMenu");
		int value = (int)sellMenu.Find("Slider").GetComponent<Slider>().value * (_selling ? -1 : 1);
		savedAboveData.collectibles[indexOfSell] += value;
		SetSliderMax();
		ChangeGold(-value * savedAboveData.dailyMineralValues[indexOfSell]);
		// don't save here to allow for mistakes (they can restart and take it back)
		// on second thought, the price for sale and buy is the same...
		//SaveData(savedAboveData);
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
		PersistentManager.PlaySoundEffect(click);
		switch ((FUNCTIONCALL)_function)
		{
			case FUNCTIONCALL.Collect:
				savedBothData.gold += currOpenBuilding.collection;
				txt.transform.position = Camera.main.WorldToScreenPoint(transform.position);
				txt.color = Color.yellow;
				currOpenBuilding.collection = 0f;
				currOpenBuilding = null;
				break;
			case FUNCTIONCALL.UpgradeMenu:
				menu.Find("Upgrade_Menu").gameObject.SetActive(true);
				CameraMovement.cam.SetCameraMode((int)CameraMovement.CAMERA_MODE.Transition);
				menuPressed(false);
				// todo :
				// turn interactable = false if we cant buy it
				break;
			case FUNCTIONCALL.ShopMenu:
				menu.Find("Shop").gameObject.SetActive(true);
				CameraMovement.cam.SetCameraMode((int)CameraMovement.CAMERA_MODE.Transition);
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
		SaveAll(savedBothData, SavedBelowData, SavedAboveData);
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

	static public void SaveAll(SaveBothGroundData _bothData, SaveAboveData _aboveData)
	{
		FileStream fs = File.Create(Application.persistentDataPath + saveBothFile);
		new BinaryFormatter().Serialize(fs, _bothData);
		fs.Close();

		SaveAboveOnly(_aboveData);
	}

	static public void SaveAll(SaveBothGroundData _bothData, SaveBelowData _belowData)
	{
		BinaryFormatter bf = new BinaryFormatter();
		FileStream fs = File.Create(Application.persistentDataPath + saveStageFile);
		bf.Serialize(fs, new StageData());
		fs.Close();

		fs = File.Create(Application.persistentDataPath + saveBothFile);
		bf.Serialize(fs, _bothData);
		fs.Close();

		fs = File.Create(Application.persistentDataPath + saveBelowFile);
		bf.Serialize(fs, _belowData);
		fs.Close();
	}

	static public void SaveAll(SaveBothGroundData _bothData, SaveBelowData _belowData, SaveAboveData _aboveData)
	{
		BinaryFormatter bf = new BinaryFormatter();

		FileStream fs = File.Create(Application.persistentDataPath + saveStageFile);
		bf.Serialize(fs, new StageData());
		fs.Close();

		fs = File.Create(Application.persistentDataPath + saveBothFile);
		bf.Serialize(fs, _bothData);
		fs.Close();

		fs = File.Create(Application.persistentDataPath + saveBelowFile);
		bf.Serialize(fs, _belowData);
		fs.Close();

		SaveAboveOnly(_aboveData);
	}

	static public void SaveAboveOnly(SaveAboveData _data)
	{
		FileStream fs = File.Create(Application.persistentDataPath + saveAboveFile);
		_data.smeltInd = mcScript.smelt.index;
		_data.craftIndex = mcScript.craft.index;
		_data.smeltTime = mcScript.smelt.lastAccessed.AddSeconds(-mcScript.smelt.currTime);
		_data.craftTime = mcScript.craft.lastAccessed.AddSeconds(-mcScript.craft.currTime);

		new BinaryFormatter().Serialize(fs, _data);
		fs.Close();
	}

	// for safety
	static public SaveAboveData SavedAboveData
	{
		get
		{
			if (savedAboveData == null)
			{
				FileStream fs = File.Open(Application.persistentDataPath + saveAboveFile, FileMode.Open);
				savedAboveData = (SaveAboveData)new BinaryFormatter().Deserialize(fs);
				fs.Close();
			}
			return savedAboveData;
		}
		set
		{
			savedAboveData = value;
		}
	}

	// for safety
	static public SaveBelowData SavedBelowData
	{
		get
		{
			if (savedBelowData == null)
			{
				BinaryFormatter bf = new BinaryFormatter();
				FileStream fs = File.Open(Application.persistentDataPath + saveBelowFile, FileMode.Open);
				savedBelowData = (SaveBelowData)bf.Deserialize(fs);
				fs.Close();
			}
			return savedBelowData;
		}
		set
		{
			savedBelowData = value;
		}
	}

	static public void CreateTile(Vector2 _pos, float depth)
	{
		Tile newTile = Instantiate(cloneTile, _pos, Quaternion.AngleAxis(90 * UnityEngine.Random.Range(0, 4), Vector3.forward)).GetComponent<Tile>();
		newTile.hp = (sbyte)FullHpForDepth(_pos.y);
		int chance = UnityEngine.Random.Range(0, 500);
		if (chance < 250)
			newTile.value = (byte)TILE_VALUES.Dirt;
		else if (chance < 290)
			newTile.value = (byte)TILE_VALUES.Stone;
		else if (chance < 340)
			newTile.value = (byte)TILE_VALUES.Oil;
		else if (chance < 345)
			newTile.value = (byte)TILE_VALUES.Map;
		else if (chance == 345)
			newTile.value = (byte)TILE_VALUES.Treasure;
		else
			// minerals
			newTile.value = (byte)Map(Mathf.Min(1000f, (depth * .5f + 2f) * Mathf.Log((chance - 345), 1.3f)), 0f, 1000f, (float)TILE_VALUES.Coal, (float)(TILE_VALUES.Total - 1));
	}

	static public void LoadNew()
	{
		Vector2 gridPos;
		uint x = uint.MaxValue; while (++x != maxWidth)
		{
			uint y = (x==0u? 0u:uint.MaxValue);
			while (++y != maxHeight)
			{
				gridPos.x = 2u * x;// 2 is the size of the objects (200 pixles, and the world is set to 100 pixels per unit)
				gridPos.y = -2u * y;
				CreateTile(gridPos, y);
			}
		}
		savedAboveData = new SaveAboveData();
		mcScript.craft.index = -1;
		mcScript.smelt.index = -1;
		savedBothData = new SaveBothGroundData();
		SaveAll(savedBothData, new SaveBelowData(), savedAboveData);
	}

	public void LoadStage()
	{
		Transform smeltMenu = menu.Find("Smelting");
		Transform content = smeltMenu.GetChild(1).GetChild(0).GetChild(0);
		if (File.Exists(Application.persistentDataPath + saveStageFile))
		{
			BinaryFormatter bf = new BinaryFormatter();
			FileStream fs = File.Open(Application.persistentDataPath + saveStageFile, FileMode.Open);
			try
			{
				StageData saveStageData = (StageData)bf.Deserialize(fs);
				fs.Close();
				// above
				fs = File.Open(Application.persistentDataPath + saveAboveFile, FileMode.Open);
				savedAboveData = (SaveAboveData)bf.Deserialize(fs);
				fs.Close();
				// both
				fs = File.Open(Application.persistentDataPath + saveBothFile, FileMode.Open);
				savedBothData = (SaveBothGroundData)bf.Deserialize(fs);
				fs.Close();

				Vector2 gridPos;
				GameObject cloneTile = Resources.Load<GameObject>("Tile");
				const int minV = -1;
				int x = saveStageData.saveTiles.Length; while (--x != minV)
				{
					StageData.SaveTile saveTile = saveStageData.saveTiles[x];
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
				x = saveStageData.saveBuildings.Length; while (--x != minV)
				{
					StageData.SaveBuilding saveBuilding = saveStageData.saveBuildings[x];
					gridPos.x = saveBuilding.xPos;
					BuildingScript newBuilding = Instantiate(cloneTile, gridPos, Quaternion.identity).GetComponent<BuildingScript>();
					newBuilding.type = saveBuilding.buildingType;
					newBuilding.efficiency = saveBuilding.efficiency;
					newBuilding.efficiency -= .0000001f * (float)(DateTime.Now - savedAboveData.currTime).TotalSeconds / Time.fixedDeltaTime;// the .0001 is from building scripts update.
					if (newBuilding.efficiency < .5f)
					{
						newBuilding.efficiency = .5f;
					}
				}
				mcScript.goldTxt.text = savedBothData.gold.ToString("F2");
				GameObject.Find("MineralCountTxt").GetComponent<UnityEngine.UI.Text>().text = "0/" + savedBothData.bagSize;
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

		// TODO:
		// after we have balanced the game, we can get rid of the unity editor objects and just create them with code
		// this will require a lot of magic numbers that I don't want to deal with right now

		Sprite[] textures = Resources.LoadAll<Sprite>("Collectibles");// all the different collectible images loaded once to prevent a bunch of resource collection
		smeltMenu.gameObject.SetActive(true);
		Button[] buyButtons = content.GetComponentsInChildren<Button>();// grabs all the buttons in order (make sure in unity to keep them in order
		GameObject costImage = Resources.Load<GameObject>("CostImage");// the individual object for every cost (I think it's easier then actually adding and changing the images manually)
		int j = -1; while (++j != buyButtons.Length)
		{
			Button buyButton = buyButtons[j];
			Transform buyBtnTransform = buyButton.transform;
			int indexOfCrafts = j + (int)COLLECTIBLES.CopperBar;
			CostScript costScript = buyButton.GetComponent<CostScript>();
			textures[indexOfCrafts].name = indexOfCrafts.ToString();
			buyBtnTransform.GetChild(0).GetComponent<Image>().sprite = textures[indexOfCrafts];// so I don't have to change them in Unity manually
			Text btnTxt = buyBtnTransform.GetChild(1).GetComponent<Text>();
			buyBtnTransform.GetChild(2).GetComponent<Text>().text = ((COLLECTIBLES)indexOfCrafts).ToString();// name
			buyButton.onClick.AddListener(() => ClickedBuyButton(buyBtnTransform));// honestly just being lazy here lol

			int available = savedAboveData.collectibles[indexOfCrafts];
			if (available == -1)
			{
				btnTxt.text = costScript.buy.ToString();
			}
			else
			{
				Destroy(buyBtnTransform.GetChild(0).GetChild(0).gameObject);
				btnTxt.text = "Craft";
			}
			//create a costimage for every cost
			int i = -1; while (++i != costScript.costs.Length)
			{
				Transform newCostImage = Instantiate(costImage, buyBtnTransform).transform;
				newCostImage.localPosition = new Vector3((((RectTransform)newCostImage.transform).offsetMin.x - ((RectTransform)newCostImage.transform).offsetMax.x) * (i + 1) - 90f, -7f);
				Image image = newCostImage.GetComponent<Image>();
				int index = (int)costScript.indices[i];
				image.sprite = textures[index];
				image.sprite.name = index.ToString();
				Text costImageTxt = newCostImage.GetChild(0).GetComponent<Text>();// this will be used for the cost in the future
				costImageTxt.text = costScript.costs[i].ToString();
				if (costScript.costs[i] > available)
				{
					costImageTxt.color = new Color(.7f, 0f, 0f, 1f);
				}
			}
			Destroy(costScript);// my genius idea so I don't have to keep this memory!
		}
		smeltMenu.gameObject.SetActive(false);
		// smelt index has to come first, bc crafting can be based off of the amounts
		if (savedAboveData.smeltInd != -1)
		{
			smelt.index = savedAboveData.smeltInd;
			smelt.lastAccessed = savedAboveData.smeltTime;
			smelt.StartCraft(content.GetChild(0).GetChild(savedAboveData.smeltInd - (int)COLLECTIBLES.CopperBar));
		}
		if (savedAboveData.craftIndex != -1)
		{
			craft.index = savedAboveData.craftIndex;
			craft.lastAccessed = savedAboveData.craftTime;
			craft.StartCraft(content.GetChild(1).GetChild(savedAboveData.craftIndex - (int)COLLECTIBLES.Graphite));
		}
	}

	static public void IncreaseResource(int _index, int _amount = 1)
	{
		if (SavedAboveData.collectibles[_index] == -1 /*&& _amount > 0/*redudant check, this should never happen*/)
		{
			SavedAboveData.collectibles[_index] = 0;
		}
		SavedAboveData.collectibles[_index] += _amount;
		if (mainMenuContent.gameObject.activeInHierarchy)
		{
			int offset;
			switch (menuType)
			{
				case MENUTYPE.Bars:
					offset = (int)COLLECTIBLES.CopperBar;
					if (_index < offset || _index > (int)COLLECTIBLES.MarsiniumBar)
					{
						return;
					}
					break;
				case MENUTYPE.Crafts:
					offset = (int)COLLECTIBLES.Graphite;
					if (_index < offset || _index > (int)COLLECTIBLES.UltimateWeapon)
					{
						return;
					}
					break;
				case MENUTYPE.Shop:
					offset = (int)COLLECTIBLES.Beam;
					break;
				default:
					offset = 0;
					if (_index > (int)COLLECTIBLES.Marsinium)
					{
						return;
					}
					break;
			}
			mainMenuContent.GetChild(0).GetChild(_index - offset).GetChild(1).GetComponent<Text>().text = SavedAboveData.collectibles[_index].ToString();// count
		}
	}

	// called from editor
	public void UpdateCraftPrices(Transform _content)
	{
		if (_content.gameObject.activeInHierarchy)
		{
			int offset = (int)(_content.GetSiblingIndex() == 0 ? COLLECTIBLES.CopperBar : COLLECTIBLES.Graphite);
			Button[] buyButtons = _content.GetComponentsInChildren<Button>();// grabs all the buttons in order (make sure in unity to keep them in order
			int j = -1; while (++j != buyButtons.Length)
			{
				Button buyButton = buyButtons[j];
				// making a map/ditionary seems way more efficient for speed purposes, but this only happens at human speed. 
				bool canClick = true;
				int i = buyButton.transform.childCount; while (--i != 3)// there are 4 children already before we add the costimages
				{
					Transform costImage = buyButton.transform.GetChild(i);
					Text costImageTxt = costImage.GetChild(0).GetComponent<Text>();
					int cost = int.Parse(System.Text.RegularExpressions.Regex.Match(costImageTxt.text, "\\d+").Value);
					//																							the index of the costimage
					int available = savedAboveData.collectibles[int.Parse(System.Text.RegularExpressions.Regex.Match(costImage.GetComponent<Image>().sprite.name, "\\d+").Value)];// might be able to remove the -1
					if (cost > available)
					{
						canClick = false;
						costImageTxt.color = Color.red;
					}
					else
					{
						costImageTxt.color = Color.white;
					}
					costImageTxt.text = cost.ToString() + '/' + (available != -1 ? available : 0);

				}
				if (savedAboveData.collectibles[j + offset] == -1)
				{
					//																		the txt on the btn (gold cost)
					canClick = int.Parse(System.Text.RegularExpressions.Regex.Match(buyButton.transform.GetChild(1).GetComponent<Text>().text, "\\d+").Value) <= savedBothData.gold;
				}
				// the button should only be clickable if you can actually unlock or start crafting.
				buyButton.interactable = canClick;
			}
		}
	}

	static public void ClickedBuyButton(Transform _buyButton)
	{
		// I think it might be faster to grab the index out of the image but im not sure
		Transform main = _buyButton.GetChild(0);
		if (main.childCount != 0)
		{
			Destroy(main.GetChild(0).gameObject);
			Text btnText = _buyButton.GetChild(1).GetComponent<Text>();
			// the cost is stored in the btn text initally until it is unlocked
			float goldAmount = int.Parse(System.Text.RegularExpressions.Regex.Match(btnText.text, "\\d+").Value);
			savedBothData.gold -= goldAmount;
			SetText("Gold -$" + goldAmount.ToString("F0"), Color.yellow, Camera.main.WorldToScreenPoint(_buyButton.position));
			// the image has the index number in its name, and we need to set the value from -1 to 0
			savedAboveData.collectibles[int.Parse(System.Text.RegularExpressions.Regex.Match(_buyButton.GetChild(0).GetComponent<Image>().sprite.name, "\\d+").Value)] = 0;
			btnText.text = "Craft";
			mcScript.UpdateCraftPrices(_buyButton.parent);// have to update all the the buttons bc you might not have enough gold to unlock them anymore
		}
		else
		{
			switch (_buyButton.parent.GetSiblingIndex())
			{
				case 0:
					mcScript.smelt.SetCraft(_buyButton);
					break;
				case 1:
					mcScript.craft.SetCraft(_buyButton);
					break;
				case 2:
					//TODO:
					// make buildings work
					break;
				default:
					break;
			}
		}
		SaveAboveOnly(savedAboveData);
	}

	static public void SetText(string _text, Color _color, Vector3 _position)
	{
		txt.text = _text;
		txt.color = _color;
		txt.transform.position = _position;
	}

	static public void ChangeGold(float _amount)
	{
		savedBothData.gold += _amount;
		mcScript.goldTxt.text = savedBothData.gold.ToString("F2");
	}

	IEnumerator CreateSparks()
	{
		GameObject bagBtn = GameObject.Find("MineralCountTxt");
		List<uint> indices = new List<uint>();
		bagBtn.GetComponent<Text>().text = "0/" + savedBothData.bagSize;
		Vector3 bagPos = bagBtn.transform.position;
		bagPos.z = 0f;
		foreach (TypeAmount item in PlayerScript.bag)
		{
			uint index = item.GetIndex();
			indices.Add(index);
			if (savedAboveData.collectibles[index] == -1)
			{
				savedAboveData.collectibles[index] = 0;
				// TODO:
				// new Animation
			}
			savedAboveData.collectibles[index] += (int)item.GetAmount();
		}
		menuType = MENUTYPE.Minerals;
		mcScript.SetMainMenu();
		PlayerScript.bag = new List<TypeAmount>();// erase the data in the bag
		Transform mainMenu = menu.Find("MainMenu");
		mainMenu.gameObject.SetActive(true);

		// create the sparks
		Transform content = mainMenuContent.GetChild(0);// the content is located pretty for down
		Vector2 offset = new Vector2(.7f, -.5f);
		GameObject sparkObj = Resources.Load<GameObject>("NewItemParticle");
		int i = -1; while (++i != indices.Count)
		{
			RandomTravelScript spark = Instantiate(sparkObj, bagPos, Quaternion.identity).GetComponent<RandomTravelScript>();
			spark.aim = content.GetChild((int)indices[i]);
			spark.offSet = offset;
			spark.timer = 70u;
			yield return new WaitForSeconds(.4f);
		}
		SaveAll(savedBothData, SavedBelowData, SavedAboveData);
		savedBelowData = null;
		yield break;
	}
}

// at this point im only using it for the player bag bc of my genius idea with the shop
// TODO:
// delete this if we dont need it
public struct TypeAmount
{
	// the first 7 to the left are the index ((max of 127))
	// the rest are the amount (max of 33,554,431)
	uint value;
	public TypeAmount(uint index, uint _amount = 0u)
	{
		value = (index << 25) | _amount;
	}
	public static implicit operator TypeAmount(uint value)
	{
		return new TypeAmount(value);
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
		value = GetIndex();
		value |= _value;
	}
	public static TypeAmount operator +(TypeAmount first, uint second)
	{
		// no bit manipulation required bc the index is on the left. (if values start going above max, we will need to do something different)
		first.value += second;
		return first;
	}
	public override bool Equals(object obj)
	{
		return (value & 0xFE000000) == (((TypeAmount)obj).value & 0xFE000000);
	}
	public override int GetHashCode()
	{
		return GetIndex().GetHashCode();
	}

}

[Serializable]
public class StageData
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
	public StageData()
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
public class SaveAboveData
{
	public int smeltInd, craftIndex;
	public int[] collectibles;
	public float[] dailyMineralValues;
	public DateTime currTime;
	public DateTime smeltTime;
	public DateTime craftTime;
	public List<int> highsAndLows;
	public SaveAboveData()
	{
		currTime = System.DateTime.Now;
		collectibles = new int[(int)MCScript.COLLECTIBLES.Total]; // items
																  // initialize to -1 so we know if they have unlocked it or not
		int i = -1; while (++i != (int)MCScript.COLLECTIBLES.Beam)
		{
			collectibles[i] = -1;
		}
		dailyMineralValues = new float[collectibles.Length];
		dailyMineralValues[(int)MCScript.COLLECTIBLES.MapPiece] = 2f;
		dailyMineralValues[(int)MCScript.COLLECTIBLES.Beam] = 2f;
		dailyMineralValues[(int)MCScript.COLLECTIBLES.Bridge] = 2f;
		dailyMineralValues[(int)MCScript.COLLECTIBLES.Teleporter] = 2f;
		dailyMineralValues[(int)MCScript.COLLECTIBLES.Marker] = 2f;
		highsAndLows = RandomizeValues();
	}
	public List<int> RandomizeValues()
	{
		int i = -1; while (++i != (int)MCScript.COLLECTIBLES.CopperBar)
		{
			dailyMineralValues[i] = Mathf.Pow(1f + UnityEngine.Random.Range(0f, .6f), i);
		}
		--i; while (++i != (int)MCScript.COLLECTIBLES.Graphite)
		{
			dailyMineralValues[i] = 200f + Mathf.Pow(2.3f + UnityEngine.Random.Range(0f, .6f), i - 31);
		}
		--i; while (++i != (int)MCScript.COLLECTIBLES.Beam)
		{
			dailyMineralValues[i] = 20f + Mathf.Pow(1.5f + UnityEngine.Random.Range(0f, .6f), i - 61);
		}

		int highOutlier = UnityEngine.Random.Range(0, (int)MCScript.COLLECTIBLES.CopperBar), lowOutlier;
		List<int> selectedHighLow = new List<int>(6);

		// minerals
		do
		{
			lowOutlier = UnityEngine.Random.Range(0, (int)MCScript.COLLECTIBLES.CopperBar);
		} while (lowOutlier != highOutlier);
		selectedHighLow.Add(highOutlier); selectedHighLow.Add(lowOutlier);
		dailyMineralValues[highOutlier] *= UnityEngine.Random.Range(1.2f, 2.3f);
		dailyMineralValues[lowOutlier] *= UnityEngine.Random.Range(.3f, .8f);

		//bars
		highOutlier = UnityEngine.Random.Range(0, (int)MCScript.COLLECTIBLES.Graphite);
		do
		{
			lowOutlier = UnityEngine.Random.Range(0, (int)MCScript.COLLECTIBLES.Graphite);
		} while (lowOutlier != highOutlier);
		selectedHighLow.Add(highOutlier); selectedHighLow.Add(lowOutlier);
		dailyMineralValues[highOutlier] *= UnityEngine.Random.Range(1.2f, 2.3f);
		dailyMineralValues[lowOutlier] *= UnityEngine.Random.Range(.3f, .8f);

		// crafts
		highOutlier = UnityEngine.Random.Range(0, (int)MCScript.COLLECTIBLES.Beam);
		do
		{
			lowOutlier = UnityEngine.Random.Range(0, (int)MCScript.COLLECTIBLES.Beam);
		} while (lowOutlier != highOutlier);
		selectedHighLow.Add(highOutlier); selectedHighLow.Add(lowOutlier);
		dailyMineralValues[highOutlier] *= UnityEngine.Random.Range(1.2f, 2.3f);
		dailyMineralValues[lowOutlier] *= UnityEngine.Random.Range(.3f, .8f);

		return selectedHighLow;
	}
}

[Serializable]
public class SaveBothGroundData
{
	public uint upgrades,/* bit field */tutorial, /* increase as you hit the spot or skip */bagSize;
	public float gold;
	public int depth;
	public uint[] missions, currTrackedMineral;// double bitfield missions can be accomplished anywhere

	public SaveBothGroundData()
	{
		bagSize = 50;
		// TODO
		// balance out costs
		depth = -5;
		missions = new uint[5];
		currTrackedMineral = new uint[5];
		missions[0] = 1;
		missions[1] = 1;
	}
}

[Serializable]
public class SaveBelowData
{
	public uint[,] tiles;// giant bitfield
	public SaveBelowData()
	{
		tiles = new uint[3,1000];
		tiles[0, 0] = 0xFC000000;
		tiles[0, 1] = 0xFC000000;
		tiles[0, 2] = 0xFC000000;
		tiles[0, 3] = 0xFC000000;
		tiles[0, 4] = 0xFC000000;
		tiles[0, 5] = 0xFC000000;
		tiles[0, 6] = 0xFC000000;
		tiles[0, 7] = 0xFC000000;
	}
}