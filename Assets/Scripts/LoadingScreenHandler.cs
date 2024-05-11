using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreenHandler : MonoBehaviour
{
    public GameObject LoadingScreen;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator FadeInFadeOutLoading()
    {
        CanvasGroup LoadingCanvas = LoadingScreen.GetComponent<CanvasGroup>();

        LoadingScreen.SetActive(true);
        LoadingCanvas.alpha = 0;

        for (float i = 0; i <= 1; i += 0.01f)
        {
            LoadingCanvas.alpha = 1 - i;
            yield return null;
        }

        yield return new WaitForSeconds(1.5f);

        for (float i = 1; i <= 0; i -= 0.01f)
        {
            LoadingCanvas.alpha = i;
            yield return null;
        }


    }
}
