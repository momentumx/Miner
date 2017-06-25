using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour {

	private IEnumerator coroutine;
	static private bool running;
	private Vector3 originalCamPos;
	public bool isPaused = false;

	void Start () {
		running = false;
	}

	public void PlayShake ( float _magnitude ) {
		if ( running && coroutine != null ) {
			StopCoroutine ( coroutine );
			transform.localPosition = originalCamPos;
			running = false;
		}
		coroutine = Shake ( _magnitude * .16f, _magnitude );
		StartCoroutine ( coroutine );
	}

	IEnumerator Shake ( float duration, float magnitude ) {
		running = true;
		float elapsed = 0.0f;

		originalCamPos = transform.localPosition;

		while ( elapsed < duration ) {
			while ( isPaused ) {
				yield return new WaitForEndOfFrame ();
			}
			elapsed += Time.deltaTime;

			float percentComplete = elapsed / duration;
			float damper = 1.0f - Mathf.Clamp(4.0f * percentComplete - 3.0f, 0.0f, 1.0f);

			// map value to [-1, 1]
			float x = Random.value * 2.0f - 1.0f;
			float y = Random.value * 2.0f - 1.0f;
			x *= magnitude * damper;
			y *= magnitude * damper;

			transform.localPosition = new Vector3 ( originalCamPos.x + x, originalCamPos.y + y, originalCamPos.z );

			yield return null;
		}
		running = false;
		transform.localPosition = originalCamPos;
	}

	static public bool Is_Shaking () {
		return running;
	}
	public void Stop_Shake () {
		if ( running ) {
			StopCoroutine ( coroutine );
			transform.localPosition = originalCamPos;
			running = false;
		}
	}
}
