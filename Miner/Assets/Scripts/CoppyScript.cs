using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void AnyFunction(System.Object _param);

public class CoppyScript : MonoBehaviour
{
	public class Condition
	{

		public delegate bool ConditionDelg(System.Object _first, System.Object _second);
		public ConditionDelg conditionMet;
		public System.Object first, second, functionParam;
		public AnyFunction callFunction;
		public UnityEngine.Object toBeDestroyed;

		public Condition(System.Object _first, System.Object _second, ConditionDelg _condition, AnyFunction _function, System.Object _functionParam, UnityEngine.Object _tobedestroyed = null)
		{
			first = _first;
			second = _second;
			conditionMet = _condition;
			functionParam = _functionParam;
			callFunction = _function;
			toBeDestroyed = _tobedestroyed;
		}
		public Condition(AnyFunction _function, System.Object _functionParam, UnityEngine.Object _tobedestroyed = null)
		{
			functionParam = _functionParam;
			callFunction = _function;
			toBeDestroyed = _tobedestroyed;
		}
		public bool ConditionMet()
		{
			if (conditionMet(first, second))
			{
				callFunction(functionParam);
				Destroy(toBeDestroyed);
				return true;
			}
			return false;
		}
	}
	List<Condition> tutCondtions = new List<Condition>();
	List<Condition> missionCondtions = new List<Condition>();
	[SerializeField]
	Transform target;
	public TypeWriter writer;
	[SerializeField]
	public UnityEngine.UI.Text depthTxt;
	static public CoppyScript coppy;
	public enum TUTORIALSTEPS
	{
		Explanation, ShowDownArrow, DownArrow, Controls, Minerals, FillBag, GoUp, CatchMe, MainMenu, ExplainShop, SmeltingBtn, TeachCraft, UnlockCopperBar, CraftCopperBar, Exit, Finish
	}

	public enum MISSIONTYPE
	{
		Minerals, Smelting, Crafting, Buildings, Depth, Artifact, Purchase

			, Total
	}

	public enum PURCHASEMISSIONS
	{
		StrongAxe, Tools, OilRig, Upgrade, BuyJems
	}

	// is this a good idea or bad? I really have no idea
	public enum UNSKIPPABLETUTORIAL
	{
		Oil, Stone, Bucket, UseBucket

			, Total
	}

	private void Awake()
	{
		coppy = this;
	}

	void Start()
	{
		writer = GetComponent<TypeWriter>();
		if (MCScript.savedBothData.tutorial == 0u)
		{
			IncreaseTutorial(TUTORIALSTEPS.Explanation);
		}
		else
		{
			writer.AddMessage("Let's Get Diggin'!");
			// start minerl/smelting/depth missions 
		}

		int i = -1; while (++i != (int)UNSKIPPABLETUTORIAL.Total)
		{
			CreateUnskippable(1u << i);
		}

	}

	void FixedUpdate()
	{
		if (target)
		{
			transform.position += (Vector3)(((Vector2)target.transform.position - (Vector2)transform.position) * .04f);
			Vector3 scale = transform.localScale;
			scale.x = Mathf.Sign(target.lossyScale.x) * Mathf.Abs(scale.x);
			transform.localScale = scale;
			if ((int)target.position.y < MCScript.savedBothData.trackingAmount[(int)MISSIONTYPE.Depth])
			{
				int newDepth = (int)(transform.position.y - 10f);
				MCScript.savedBothData.trackingAmount[(int)MISSIONTYPE.Depth] = newDepth;// to prevent from calling a bunch of times
			}
		}
		int i = -1; while (++i != tutCondtions.Count)
		{
			if (tutCondtions[i].ConditionMet())
			{
				tutCondtions.RemoveAt(i);
				--i;
			}
		}
		i = -1; while (++i != missionCondtions.Count)
		{
			if (missionCondtions[i].ConditionMet())
			{
				missionCondtions.RemoveAt(i);
				--i;
			}
		}
		depthTxt.text = Mathf.Max(-transform.position.y,0f).ToString("F0");
		depthTxt.transform.parent.position = transform.GetChild(0).GetChild(1).position;
	}

	static public uint GetVision()
	{
		return MCScript.savedBothData.upgrades & 0x03u;
	}

	// thses functions take ints bc unitys buttons are stupid and cant recognize enums
	public void IncreaseTutorial(System.Object _step)
	{
		if ((uint)(TUTORIALSTEPS)_step >= MCScript.savedBothData.tutorial)
		{
			switch ((TUTORIALSTEPS)_step)
			{
				case TUTORIALSTEPS.Explanation:
					writer.AddMessage("I'm COPPY the copter. S.A.T. hired me to help you.");
					writer.AddMessage("What does S.A.T. stand for? You FORGOT!!? Ugh... I'll tell you later.");
					writer.AddMessage("Anyways... we need to find MARSINIUM here on Mars.");
					writer.AddMessage("Let's get to it. Press the Down arrow to start digging!", IncreaseTutorial, TUTORIALSTEPS.ShowDownArrow);
					GameObject.Find("goDownBtn").transform.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => IncreaseTutorial(TUTORIALSTEPS.DownArrow));
					break;
				case TUTORIALSTEPS.ShowDownArrow:
					GameObject goDown = GameObject.Find("goDownBtn");
					if (goDown)
					{
						target = goDown.transform;
					}
					break;
				case TUTORIALSTEPS.DownArrow:
					GameObject.Find("Canvas").transform.Find("Panel").Find("upperMenu").Find("goDownBtn").GetComponent<UnityEngine.UI.Button>().onClick.RemoveListener(() => IncreaseTutorial(TUTORIALSTEPS.DownArrow));
					target = GameObject.Find("Player").transform.Find("CoppyShoulder");
					writer.AddMessage("Just swipe in the direction you want to move.");
					tutCondtions.Add(new Condition(target.parent.parent.GetComponent<PlayerScript>(), -1, (x, y) => ((PlayerScript)x).digDirection != (int)y
					, IncreaseTutorial, TUTORIALSTEPS.Controls, Instantiate(Resources.Load<GameObject>("SwipeHand"), GameObject.Find("Canvas").transform)));
					break;
				case TUTORIALSTEPS.Controls:
					writer.AddMessage("Move into a block to start mining! You can mine in any direction!", IncreaseTutorial, TUTORIALSTEPS.Minerals);
					tutCondtions.Add(new Condition(target.parent.parent, 1.1f, (x, y) => ((Transform)x).position.y > (float)y, IncreaseTutorial, TUTORIALSTEPS.MainMenu));
					break;
				case TUTORIALSTEPS.Minerals:
					Tile[] currTiles = FindObjectsOfType<Tile>();
					Tile resourceTile = null;
					float dis = 100000f;
					foreach (Tile tile in currTiles)
					{
						if (tile.value > (int)MCScript.TILE_VALUES.Treasure)
						{
							float newDis = Vector2.SqrMagnitude(tile.transform.position - target.position);
							if (newDis < dis)
							{
								dis = newDis;
								resourceTile = tile;
							}
						}
					}
					target = resourceTile.transform;
					writer.AddMessage("Come Collect these resources!");
					writer.AddMessage("We can build the things we need with them!");
					writer.AddMessage("They're also worth money up top! The deeper you go the rarer they get!");
					tutCondtions.Add(new Condition(resourceTile, 2, (x, y) => ((Tile)x).hp < (int)y, IncreaseTutorial, TUTORIALSTEPS.FillBag));
					break;
				case TUTORIALSTEPS.FillBag:
					writer.AddMessage("Sweet! Now lets fill up my bag with wonderful resources!");
					writer.AddMessage("You can click the bag in the upper left to see what sweet stuff you got!");
					target = (GameObject.Find("Player").transform.Find("CoppyShoulder"));
					tutCondtions.Add(new Condition(GameObject.Find("MineralCountTxt").GetComponent<UnityEngine.UI.Text>()
						, "50", (x, y) => System.Text.RegularExpressions.Regex.Match(((UnityEngine.UI.Text)x).text, "\\d+").Value == (string)y
						, IncreaseTutorial, TUTORIALSTEPS.GoUp));
					break;
				case TUTORIALSTEPS.GoUp:
					writer.AddMessage("I can only carry 50 things. Increase my storage!", IncreaseTutorial, TUTORIALSTEPS.CatchMe);
					writer.AddMessage("Come on up! Oh, didn't I tell you? S.A.T. gave you ROCKET BOOTS!");
					break;
				case TUTORIALSTEPS.CatchMe:
					target = GameObject.Find("MineralCountTxt").transform;
					break;
				case TUTORIALSTEPS.MainMenu:
					target = GameObject.Find("Player").transform.Find("CoppyShoulder");
					writer.AddMessage("When you come up I'll put your resources in the bank!", IncreaseTutorial, TUTORIALSTEPS.ExplainShop);
					MCScript.menu.Find("MainMenu").GetChild(0).GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => IncreaseTutorial(TUTORIALSTEPS.Finish));
					MCScript.SavedAboveData.collectibles[(int)MCScript.COLLECTIBLES.Copper] += 5;
					MCScript.menu.Find("MainMenu").Find("Main").Find("SmeltingBtn").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => IncreaseTutorial(TUTORIALSTEPS.TeachCraft));
					break;
				case TUTORIALSTEPS.ExplainShop:
					target = MCScript.menu;
					writer.AddMessage("In this menu you can buy/sell any resource that you have found!");
					writer.AddMessage("Once you have enough of a resource, you can create new things!");
					writer.AddMessage("Try it out! Press the smelt button!", IncreaseTutorial, TUTORIALSTEPS.SmeltingBtn);
					break;
				case TUTORIALSTEPS.SmeltingBtn:
					target = MCScript.menu.Find("MainMenu").Find("Main").Find("SmeltingBtn");
					writer.AddMessage("This one!");
					break;
				case TUTORIALSTEPS.TeachCraft:
					writer.AddMessage("You first have to unlock it with money, and then you can create it!");
					writer.AddMessage("Unlock the CopperBar, and create it!");
					target = MCScript.menu.Find("MainMenu").Find("Scroll View");
					MCScript.menu.Find("MainMenu").Find("Main").Find("SmeltingBtn").GetComponent<UnityEngine.UI.Button>().onClick.RemoveListener(() => IncreaseTutorial(TUTORIALSTEPS.TeachCraft));
					// this gets passed by ref!!!!!!! so it will change with it
					MCScript.menu.Find("Smelting").GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => IncreaseTutorial((TUTORIALSTEPS)MCScript.savedBothData.tutorial));
					MCScript.menu.Find("Smelting").GetChild(0).GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => IncreaseTutorial(TUTORIALSTEPS.Exit));
					break;
				case TUTORIALSTEPS.CraftCopperBar:
					writer.AddMessage("Now you just have to wait for it to complete! You can Exit.");
					MCScript.menu.Find("Smelting").GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<UnityEngine.UI.Button>().onClick.RemoveListener(() => IncreaseTutorial((int)MCScript.savedBothData.tutorial));
					break;
				case TUTORIALSTEPS.Exit:
					MCScript.menu.Find("Smelting").GetChild(0).GetComponent<UnityEngine.UI.Button>().onClick.RemoveListener(() => IncreaseTutorial(TUTORIALSTEPS.Exit));
					writer.AddMessage("Each day (~5 min) Each mineral will change it's value!");
					writer.AddMessage("Make suer you buy and sell when their values are right!");
					break;
				case TUTORIALSTEPS.Finish:
					target = GameObject.Find("Player").transform.Find("CoppyShoulder");
					writer.AddMessage("That's all I have for you! Let's get back to diggin'!");
					// start minerl/smelting/depth missions and explain missions on down button
					break;
				default:
					break;
			}
			MCScript.savedBothData.tutorial = (uint)(TUTORIALSTEPS)_step + 1u;

		}

	}

	public void CreateUnskippable(System.Object _toComplete)// most likely only ever going to be 1 or 0
	{
		if ((MCScript.savedBothData.unskippableTutorial & (uint)_toComplete) == 0)
		{
			Condition newCondition = new Condition(CompletedUnskippable, (UNSKIPPABLETUTORIAL)(uint)_toComplete);
			// not completed, need to create
			switch ((UNSKIPPABLETUTORIAL)(uint)_toComplete)
			{
				case UNSKIPPABLETUTORIAL.Oil:
					newCondition.first = FindObjectOfType<PlayerScript>();
					newCondition.second = MCScript.TILE_VALUES.Oil;
					newCondition.conditionMet = (x, y) => ((PlayerScript)x).defender.value == (int)y;
					break;
				case UNSKIPPABLETUTORIAL.Stone:
					newCondition.first = FindObjectOfType<PlayerScript>().transform;
					newCondition.second = 3;//hp
					int lMask = LayerMask.NameToLayer("Stone");
					newCondition.conditionMet = (x, y) => Physics2D.Raycast((Vector2)((Transform)x).position + Vector2.down, Vector2.zero, 100f, lMask);
					break;
				default:
					break;
			}
			if (newCondition.conditionMet != null)
			{
				missionCondtions.Add(newCondition);
			}
		}
	}

	public void CompletedUnskippable(System.Object _completedBit)
	{
		if ((MCScript.savedBothData.unskippableTutorial & (uint)_completedBit) == 0)
		{
			MCScript.savedBothData.unskippableTutorial |= (uint)_completedBit;
			MCScript.SaveBothOnly();
			switch ((UNSKIPPABLETUTORIAL)_completedBit)
			{
				case UNSKIPPABLETUTORIAL.Oil:
					writer.AddMessage("This oil is going to make a mess!");
					writer.AddMessage("Build an oil rig to dig it up.");
					break;
				case UNSKIPPABLETUTORIAL.Stone:
					writer.AddMessage("Stones are un-mine-able. You need a building to get rid of it.");
					break;
				case UNSKIPPABLETUTORIAL.Bucket:
					writer.AddMessage("Don't let the stone squash you! Place a Bucket by tapping!", (x) => Time.timeScale = 0f);
					// create press detector and unfreeze when pressed
					break;
				default:
					break;
			}
		}
	}

	public void CreateMineralMission(System.Object _completed)// most likely only ever going to be 1 or 0
	{
		MCScript.Gained += CompletedMineralMission;
		// create new mission indication next to coppy
		// make it so that when they click on coppy, this gives details

	}

	bool MissionCheck(int _index, int _amount, MISSIONTYPE _type)
	{
		int missionType = (int)_type;
		if (_index == MCScript.savedBothData.missions[missionType])
		{
			MCScript.savedBothData.trackingAmount[missionType] += _amount;
			if (MCScript.savedBothData.trackingAmount[missionType] > 500)// amount for all minerals to gather (remember they could potentionally get 15)
			{
				return true;
			}
		}
		return false;
	}

	void CompletedMineralMission(int _index, int _amount)
	{

		if (MissionCheck(_index, _amount, MISSIONTYPE.Minerals))
		{
			// create completed mission indication on coppy
			// when misison open, they can collect their reward when they click on the completed mission
		}
	}

	public void CreateArtifactMission(System.Object _completed)// most likely only ever going to be 1 or 0
	{

	}
}
