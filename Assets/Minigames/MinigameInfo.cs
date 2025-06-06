using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Minigame Info", menuName = "Minigames/MinigameInfo")]
public class MinigameInfo : ScriptableObject
{
    [Header("Info")]
    public string Name;
    public Sprite Icon;
    [TextArea(3,2)] public string Description;
    [TextArea(3, 2)] public string HowToPlay;
    public Sprite[] DisplayImages;

    [Header("Scene")]
    public string MG_SceneName;

}
