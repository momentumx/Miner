using UnityEngine;

public class DmgTextScript : MonoBehaviour {
	[SerializeField]
	UnityEngine.UI.Text txt;

	void FixedUpdate () {
		
		if ( txt.color.a != 0 ) {
			Color temp = txt.color;
			temp.a -= .003f;
			txt.color = temp;
			Vector2 upDir = transform.position;
			++upDir.y;
			transform.position = upDir;
		}
	}
}
