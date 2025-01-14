using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RainbowTextCycle : MonoBehaviour
{
    TMP_Text text;
    public float speed = 1;
    [HideInInspector] public float i = 0;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    // Start is called before the first frame update
    void Start()
    {
        StartIncrement();
    }

    public void StartIncrement(float inc = 0)
    {
        if (inc > 1)
        {
            inc = 1 - inc;
        }
        else if (inc < 0)
        {
            inc = 1 + inc;
        }


        StopAllCoroutines();
        StartCoroutine(RainbowCycle(inc));
    }

    IEnumerator RainbowCycle(float inc = 0){
        i = inc;

        while(i < 1)
        {
            text.color = Color.HSVToRGB(i, 1, 1);
            yield return null;
            i += 0.01f * speed;
        }

        StartCoroutine(RainbowCycle());
    }
}
