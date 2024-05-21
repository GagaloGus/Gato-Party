using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SMG_InfoDisplay : MonoBehaviour
{
    public Sprite[] info_buttonSprites;
    public Transform Info_Display;
    Transform Panel, Buttons;

    // Start is called before the first frame update
    void Start()
    {
        Panel = Info_Display.Find("Panel");
        Buttons = Info_Display.Find("Buttons");

        TogglePanel(0);

        for (int i = 0; i < Buttons.childCount; i++)
        {
            int index = i;
            Buttons.GetChild(i).GetComponent<Button>().onClick.AddListener(() => TogglePanel(index));
        }
    }

    public void TogglePanel(int index)
    {
        if(index >= Panel.childCount)
        {
            Debug.LogWarning($"index was higher than child count: {index} / {Panel.childCount}");
            return;
        }


        for (int i = 0; i < Panel.childCount; i++)
        {
            Panel.GetChild(i).gameObject.SetActive(i == index);
        }

        Buttons.GetComponent<Image>().sprite = info_buttonSprites[index];
    }
}
