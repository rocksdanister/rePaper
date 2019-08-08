using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RGBController : MonoBehaviour {

    public Color[] colors;

    Material mat;
	// Use this for initialization
	void Start () {
        mat = GetComponent<SpriteRenderer>().material;
        StartCoroutine(Transitions());
	}

    Color tmpColor;
    /*
    IEnumerator Lightning()
    {
       // Debug.Log("lightning_a on");
        while (true)
        {
            if (true)
            {
                yield return new WaitForSeconds(2f); // delay before starting 
                tmpColor = wallpaper.color;

                yield return StartCoroutine(ColorTransition(wallpaper.color, lightning, 0.2f));
                isLightning = true;
                //yield return new WaitForSeconds(0.01f);

                //wallpaper.color = tmpColor;
                yield return StartCoroutine(ColorTransition(wallpaper.color, tmpColor, 0.1f));
                isLightning = false;

                yield return new WaitForSeconds(Random.Range(3f, 7f));
            }
            yield return null;
        }

    }
    */

    IEnumerator Transitions()
    {
        int i;
        mat.SetColor("_Color", colors[0]);
        while(true)
        {
            for (i = 0; i < 8; i++) {
                yield return StartCoroutine( ColorTransition(mat.GetColor("_Color"), colors[i],0.1f) );
            }
        }

    }

    IEnumerator ColorTransition(Color a, Color b, float incr = 0.1f, bool is_this_pending_transition = false)  // a->b
    {
        float t = 0;
        while (t <= 1)
        {
            yield return new WaitForSeconds(0.05f);
            t += incr;
            mat.SetColor("_Color", Color.Lerp(a, b, t));
            //wallpaper.color = Color.Lerp(a, b, t);
        }

        yield return null;
    }


    // Update is called once per frame
    void Update () {

	}
}
