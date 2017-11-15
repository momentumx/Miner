using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CycleLevel : MonoBehaviour {

    //public Sprite[] levels;//= new Sprite[11];
    //public Image level_image;
    //public Text level_text;
	//static public sbyte current_level;
    //sbyte max_level;
	// Use this for initialization
	//void Start () {
    //    levels = Resources.LoadAll<Sprite>("Levels");
    //    max_level = (sbyte)PlayerPrefs.GetInt("Level");
	//
    //    level_text.text = "Level: " + (max_level);
    //    level_image.sprite = levels[max_level];
    //    current_level = max_level;
    //}
	//
    //public void Cycle(int _val)
    //{
    //    current_level +=(sbyte)_val;
    //    if ( current_level < 0 )
    //        current_level = max_level;
    //    else if ( current_level > 0 )
    //        current_level = 0;
    //    level_image.sprite = levels[current_level];
    //    level_text.text = "Level: " + (current_level);
    //}
	public void StartGame()
	{
		PersistentManager.mmScript.ChangeScene(0);
	}
}
