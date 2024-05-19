using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DigitalTimerDisplay : MonoBehaviour
{
    public Transform TimerDisplay;
    Image Tens, Units;

    List<Sprite> numberSprites = new List<Sprite>();

    private void Awake()
    {
        Tens = TimerDisplay.Find("tens").GetComponent<Image>();
        Units = TimerDisplay.Find("units").GetComponent<Image>();

        //carga los sprites de los numeros
        numberSprites.Clear();
        for (int i = 0; i <= 9; i++)
        {
            numberSprites.Add(Resources.Load<Sprite>($"ClockSprites/{i}"));
        }
    }

    public void ChangeNumber(int number)
    {
        int tens = number / 10;
        int units = number % 10;

        Tens.sprite = numberSprites[tens];
        Units.sprite = numberSprites[units];
    }
}
