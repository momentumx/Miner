using UnityEngine;

public class HealthbarScript : MonoBehaviour {

    UnityEngine.UI.Slider slider;
    //// Use this for initialization
    //void Start () {
    //       prevManna = 200;
    //       circle = transform.GetChild(0).GetComponent<MeshRenderer>().material;
    //       glare = circle.GetColor("_TintColor");
    //       flashlight = transform.GetChild(1);
    //       bar = transform.GetChild(2);
    //       effect = bar.GetChild(0).GetChild(0).transform;
    //       redbar = transform.GetChild(3);
    //       //circle.SetColor("_TintColor", new Color(0,0,0));
    //   }

    void Start()
    {
        slider = GetComponent<UnityEngine.UI.Slider>();
    }


    //// Update is called once per frame
    //void FixedUpdate () {
    //       //174 38
    //       //23 80

    //       if (timer2 != 0)
    //       {
    //           redbar.localScale = new Vector3((diff-prevManna) * .005f, redbar.localScale.y, redbar.localScale.z);
    //           redbar.position = new Vector3(effect.position.x, redbar.position.y, redbar.position.z);

    //           --timer2;
    //           if (timer2 == 0)
    //           {
    //               diff = prevManna;
    //               GameObject temp = Instantiate(redbar.gameObject);
    //               temp.AddComponent<dropscript>();
    //               temp.transform.rotation = redbar.rotation;
    //               temp.transform.position = redbar.position;
    //               temp.transform.localScale = redbar.lossyScale;
    //               redbar.gameObject.SetActive(false);
    //           }
    //       }
    //       if(Movement.mana>200)
    //           Movement.mana = 200;

    //       if (.9f < prevManna - Movement.mana)
    //       {
    //           if(timer2 == 0)
    //           {
    //               diff = prevManna;
    //               redbar.localScale = new Vector3((diff - prevManna) * .005f, redbar.localScale.y, redbar.localScale.z);
    //               redbar.gameObject.SetActive(true);
    //           }
    //           timer2 = 40;
    //       }
    //       prevManna = Movement.mana;
    //       bar.localScale = new Vector3(Movement.mana * .005f, bar.localScale.y, bar.localScale.z);

    //       if (flip)
    //           timer += .02f;
    //       else
    //           timer -= .02f;
    //       if (timer > 1 || timer < 0)
    //       {
    //           flip = !flip;
    //           flashlight.position = new Vector3(transform.position.x - 4, flashlight.position.y, flashlight.position.z);
    //       }
    //       flashlight.position = new Vector3(flashlight.position.x + .1f, flashlight.position.y, flashlight.position.z);
    //       glare.a = (byte)(10 + 40 * timer);
    //       circle.SetColor("_TintColor", glare);


    //   }
}
