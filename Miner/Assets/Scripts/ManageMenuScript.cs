using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ManageMenuScript : MonoBehaviour {
	enum TRANSITION {
		None,
		Fade
	}
	[SerializeField]
	TRANSITION transition;
	AsyncOperation aOp;
	static public GameObject loadingScreen;
	[SerializeField]
	RawImage fader;
	[SerializeField]
	GameObject backBtn;
	[SerializeField]
	Transform levelSelect;
	int tempLevel;

	void Start () {
		tempLevel = PlayerPrefs.GetInt ( MasterControllerScript.SAVEKEYINT.Level.ToString () );
		levelSelect.transform.GetChild ( 1 ).GetComponent<Button> ().interactable = false;
		Texture[] levelimages = Resources.LoadAll<Texture>("LevelImages");
		levelSelect.GetChild ( 2 ).GetChild ( 2 ).GetComponent<RawImage> ().texture = levelimages [ Random.Range ( 0, levelimages.Length ) ];
		//string tempverse = Resources.Load<TextAsset> ( "Bible" ).text.Split ( '\n' ) [ 0 ];
		levelSelect.GetChild ( 2 ).GetChild ( 0 ).GetComponent<Text> ().text = "Level: " + tempLevel;
		if ( tempLevel == 0 )
			levelSelect.GetChild ( 0 ).GetComponent<Button> ().interactable = false;

	}

	void LoadScreen ( string _sceneName ) {
		Time.timeScale = 1;
		if ( GameObject.Find ( "Menus" ) )
			GameObject.Find ( "Menus" ).SetActive ( false );
		if ( transition == TRANSITION.Fade )
			fader.CrossFadeAlpha ( 1f, 2f, true );
		if ( loadingScreen ) {
			loadingScreen.SetActive ( true );
			StartCoroutine ( LoadLevelWithProgressBar ( _sceneName ) );
		} else
			SceneManager.LoadSceneAsync ( _sceneName );
	}

	public void SetMusicVol ( float _vol ) {
		MasterControllerScript.musicVol = _vol;
		MasterControllerScript.SetSaveKey ( MasterControllerScript.SAVEKEYFLT.MusicVolume, _vol );
	}

	public void SetSFXVol ( float _vol ) {
		MasterControllerScript.sfxVol = _vol;
		MasterControllerScript.SetSaveKey ( MasterControllerScript.SAVEKEYFLT.SfxVolume, _vol );
	}

	public void ChangeMusic ( int _music ) {
		MasterControllerScript.SwitchMusic ( ( MasterControllerScript.MUSIC_CLIP )_music );
	}

	public void ChangeScene ( string _sceneName ) {
		if ( _sceneName == "" )
			_sceneName = "Level" + tempLevel;
		LoadScreen ( _sceneName );
	}

	public void ChangeScene ( int _level ) {
		LoadScreen ( "Level" + _level );
	}

	public void ChangeScene () {
		LoadScreen ( "Level" + tempLevel );
	}

	public void IncLevel ( int _dir ) {
		tempLevel += _dir;
		Transform left = levelSelect.GetChild ( 0 ), right = levelSelect.GetChild ( 1 ), start = levelSelect.GetChild ( 2 );

		if ( _dir > 0 ) {
			levelSelect.GetComponent<Animator> ().SetTrigger ( "Forward" );
			right.GetComponent<RawImage> ().texture = start.GetChild ( 2 ).GetComponent<RawImage> ().texture;
			left.GetComponent<Button> ().interactable = true;
			if ( tempLevel >= PlayerPrefs.GetInt ( MasterControllerScript.SAVEKEYINT.Level.ToString () ) ) {
				right.GetComponent<Button> ().interactable = false;
				tempLevel = PlayerPrefs.GetInt ( MasterControllerScript.SAVEKEYINT.Level.ToString () );
			}
		} else {
			levelSelect.GetComponent<Animator> ().SetTrigger ( "Backward" );
			left.GetComponent<RawImage> ().texture = start.GetChild ( 2 ).GetComponent<RawImage> ().texture;
			right.GetComponent<Button> ().interactable = true;
			if ( tempLevel <= 0 ) {
				left.GetComponent<Button> ().interactable = false;
				tempLevel = 0;
			}
		}
		Texture[] levelimages = Resources.LoadAll<Texture>("LevelImages");
		start.GetChild ( 2 ).GetComponent<RawImage> ().texture = levelimages [ Random.Range ( 0, levelimages.Length ) ];
		start.GetChild ( 0 ).GetComponent<Text> ().text = "Level: " + tempLevel;


	}

	public void ChangeMenu ( GameObject _currMenu ) {
		if ( _currMenu ) {
			GameObject allMenus = GameObject.Find ( "Menus" );
			int childs = allMenus.transform.childCount;
			int i =-1; while ( ++i != childs ) {
				GameObject temp = allMenus.transform.GetChild ( i ).gameObject;
				if ( temp != _currMenu )
					temp.SetActive ( false );
				else
					temp.SetActive ( true );
			}
			if ( _currMenu.name != "Main" ) {
				backBtn.SetActive ( true );
			} else {
				backBtn.SetActive ( false );
			}
		}

	}

	IEnumerator LoadLevelWithProgressBar ( string _sceneName ) {
		yield return new WaitForSeconds ( 1 );//avoids loading conflicts

		aOp = SceneManager.LoadSceneAsync ( _sceneName );

		aOp.allowSceneActivation = false;
		Slider loadingProgBar = null;
		if ( loadingScreen )
			loadingProgBar = loadingScreen.transform.GetChild ( 0 ).GetComponent<Slider> ();
		while ( !aOp.isDone ) {
			if ( loadingProgBar )
				loadingProgBar.value = aOp.progress;

			if ( aOp.progress == 0.9f ) {
				if ( loadingProgBar )
					loadingProgBar.value = 1.0f;
				aOp.allowSceneActivation = true;
			}

			yield return null;
		}

	}

	public static void Win () {

	}

}
