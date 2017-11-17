using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void AnyFunction(int _param);

public class CoppyScript : MonoBehaviour
{
	class Condition
	{

		public delegate bool ConditionDelg(System.Object _first, System.Object _second);
		ConditionDelg conditionMet;
		System.Object first, second;
		AnyFunction callFunction;
		int functionParam;
		UnityEngine.Object toBeDestroyed;

		public Condition(System.Object _first, System.Object _second, ConditionDelg _condition, AnyFunction _function, int _functionParam, UnityEngine.Object _tobedestroyed = null)
		{
			first = _first;
			second = _second;
			conditionMet = _condition;
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
	Transform target;
	TypeWriter writer;
	public enum TUTORIALSTEPS
	{
		Explanation, ShowDownArrow, DownArrow, Controls, Minerals, FillBag, GoUp, CatchMe, MainMenu, ExplainShop, SmeltingBtn, TeachCraft, UnlockCopperBar, CraftCopperBar, Exit, Finish
	}

	void Start()
	{
		target = GameObject.Find("Player").transform.GetChild(1);
		writer = GetComponent<TypeWriter>();
		if (MCScript.savedBothData.tutorial == 0u)
		{
			IncreaseTutorial((int)TUTORIALSTEPS.Explanation);
		}
		else
		{
			writer.AddMessage("Let's Get Diggin'!");
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
			if ((int)target.position.y < MCScript.savedBothData.depth)
			{
				MCScript.savedBothData.depth = (int)(transform.position.y - 2.5f);// to prevent from calling a bunch of times
				writer.AddMessage("Wow! new Depth reached: " + Mathf.Abs(MCScript.savedBothData.depth) * 5);
			}
		}
		int i = -1; while (++i!= tutCondtions.Count)
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
	}

	static public uint GetVision()
	{
		return MCScript.savedBothData.upgrades & 0x03u;
	}

	public void IncreaseMission()
	{
		MCScript.savedBothData.missions[0] = MCScript.savedBothData.missions[0] <<1;
	}

	public void CreateMission()
	{
		if (true)
		{

		}
	}
	
	public void IncreaseTutorial( int _step)
	{
		if ((uint)_step >= MCScript.savedBothData.tutorial)
		{
			switch ((TUTORIALSTEPS)_step)
			{
				case TUTORIALSTEPS.Explanation:
					writer.AddMessage("I'm COPPY the copter. I was Hired by S.A.T. The company that hired you.");
					writer.AddMessage("What does S.A.T. stand for? You FORGOT!!? Ugh... I'll tell you later.");
					writer.AddMessage("Anyways... You were hired to find MARSINIUM here on Mars.");
					writer.AddMessage("Maybe if we find some we can figure out why Mars is doing SO poorly.");
					writer.AddMessage("Let's get to it shall we. Press the Down arrow to start digging!", IncreaseTutorial, (int)TUTORIALSTEPS.ShowDownArrow);
					GameObject.Find("goDownBtn").transform.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => IncreaseTutorial((int)TUTORIALSTEPS.DownArrow));
					break;
				case TUTORIALSTEPS.ShowDownArrow:
					GameObject goDown = GameObject.Find("goDownBtn");
					if (goDown)
					{
						target = goDown.transform;
					}
					break;
				case TUTORIALSTEPS.DownArrow:
					GameObject.Find("Canvas").transform.GetChild(1).GetChild(6).GetChild(0).GetComponent<UnityEngine.UI.Button>().onClick.RemoveListener(() => IncreaseTutorial((int)TUTORIALSTEPS.DownArrow));
					target = GameObject.Find("Player").transform.Find("CoppyShoulder");
					writer.AddMessage("To move around this place just swipe in the direction you want to move.");
					tutCondtions.Add(new Condition(target.parent.parent.GetComponent<PlayerScript>(), -1, (x, y) => ((PlayerScript)x).digDirection != (int)y
					, IncreaseTutorial, (int)TUTORIALSTEPS.Controls, Instantiate(Resources.Load<GameObject>("SwipeHand"), GameObject.Find("Canvas").transform)));
					break;
				case TUTORIALSTEPS.Controls:
					writer.AddMessage("Move into a block to start mining! You can mine in any direction!", IncreaseTutorial, (int)TUTORIALSTEPS.Minerals);
					tutCondtions.Add(new Condition(target.parent.parent, 1.1f, (x, y) => ((Transform)x).position.y > (float)y, IncreaseTutorial, (int)TUTORIALSTEPS.MainMenu));
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
					tutCondtions.Add(new Condition(resourceTile, 2, (x, y) => ((Tile)x).hp < (int)y, IncreaseTutorial, (int)TUTORIALSTEPS.FillBag));
					break;
				case TUTORIALSTEPS.FillBag:
					writer.AddMessage("Sweet! Now lets fill up my bag with wonderful resources!");
					writer.AddMessage("You can click the bag in the upper left to see what sweet stuff you got!");
					target = (GameObject.Find("Player").transform.Find("CoppyShoulder"));
					tutCondtions.Add(new Condition(GameObject.Find("MineralCountTxt").GetComponent<UnityEngine.UI.Text>()
						, "50", (x, y) => System.Text.RegularExpressions.Regex.Match(((UnityEngine.UI.Text)x).text, "\\d+").Value == (string)y
						, IncreaseTutorial, (int)TUTORIALSTEPS.GoUp));
					break;
				case TUTORIALSTEPS.GoUp:
					writer.AddMessage("Right now I can carry 50 things. Increase my storage later!", IncreaseTutorial, (int)TUTORIALSTEPS.CatchMe);
					writer.AddMessage("Come on up! Oh, didn't I tell you? S.A.T. gave you ROCKET BOOTS!");
					break;
				case TUTORIALSTEPS.CatchMe:
					target = GameObject.Find("MineralCountTxt").transform;
					break;
				case TUTORIALSTEPS.MainMenu:
					target = GameObject.Find("Player").transform.Find("CoppyShoulder");
					writer.AddMessage("When you come up I'll put your resources in the bank!", IncreaseTutorial, (int)TUTORIALSTEPS.ExplainShop);
					MCScript.menu.Find("MainMenu").GetChild(0).GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => IncreaseTutorial((int)TUTORIALSTEPS.Finish));
					MCScript.SavedAboveData.collectibles[(int)MCScript.COLLECTIBLES.Copper] += 5;
					MCScript.menu.Find("MainMenu").Find("SmeltingBtn").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => IncreaseTutorial((int)TUTORIALSTEPS.TeachCraft));
					break;
				case TUTORIALSTEPS.ExplainShop:
					target = MCScript.menu;
					writer.AddMessage("In this menu you can buy/sell any resource that you have found!");
					writer.AddMessage("Once you have enough of a resource, you can create new things!");
					writer.AddMessage("Try it out! Press the smelt button!", IncreaseTutorial, (int)TUTORIALSTEPS.SmeltingBtn);
					break;
				case TUTORIALSTEPS.SmeltingBtn:
					target = MCScript.menu.Find("MainMenu").Find("SmeltingBtn");
					writer.AddMessage("Press Me!");
					break;
				case TUTORIALSTEPS.TeachCraft:
					writer.AddMessage("You first have to unlock it with money, and then you can create it!");
					writer.AddMessage("Unlock the CopperBar, and create it!");
					target = MCScript.menu.Find("MainMenu").Find("Scroll View");
					MCScript.menu.Find("MainMenu").Find("SmeltingBtn").GetComponent<UnityEngine.UI.Button>().onClick.RemoveListener(() => IncreaseTutorial((int)TUTORIALSTEPS.TeachCraft));
					// this gets passed by ref!!!!!!! so it will change with it
					MCScript.menu.Find("Smelting").GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => IncreaseTutorial((int)MCScript.savedBothData.tutorial));
					MCScript.menu.Find("Smelting").GetChild(0).GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => IncreaseTutorial((int)TUTORIALSTEPS.Exit));
					break;
				case TUTORIALSTEPS.CraftCopperBar:
					writer.AddMessage("Now you just have to wait for it to complete! You can Exit.");
					MCScript.menu.Find("Smelting").GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<UnityEngine.UI.Button>().onClick.RemoveListener(() => IncreaseTutorial((int)MCScript.savedBothData.tutorial));
					break;
				case TUTORIALSTEPS.Exit:
					MCScript.menu.Find("Smelting").GetChild(0).GetComponent<UnityEngine.UI.Button>().onClick.RemoveListener(() => IncreaseTutorial((int)TUTORIALSTEPS.Exit));
					writer.AddMessage("Each day (~5 min) Each mineral will change it's value!");
					writer.AddMessage("Make suer you buy and sell when their values are right!");
					break;
				case TUTORIALSTEPS.Finish:
					target = GameObject.Find("Player").transform.Find("CoppyShoulder");
					writer.AddMessage("That's all I have for you! Let's get back to diggin'!");
					break;
				default:
					break;
			}
			MCScript.savedBothData.tutorial = (uint)++_step;

		}

	}

}
