using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class ManaUpgradeMenu : MonoBehaviour 
{
    public Text manaText;
	// Use this for initialization
	void Start () 
	{
        //manaText.text = (Player.mana / Player.maxMana * 100).ToString("F0") + "% (" + Player.mana.ToString ( "F0" ) + ')';
	}

	public void Buy (UnityEngine.Object _btn)
	{
        //UpgradeValueScript _val = ((GameObject)_btn).GetComponent<UpgradeValueScript>();
        //if ( PlayerPrefs.GetInt ( _val.saveKey.ToString () ) < 5 && Player.mana >= _val.cost ) {
		//	Player.mana -= _val.cost;
        //    //manaText.text = (Player.mana / Player.maxMana * 100).ToString("F0") + "% (" + Player.mana.ToString ( "F0" ) + ')';
        //    _val.slider.value = MasterControllerScript.SaveKeyInc(_val.saveKey);
        //}
	}
}