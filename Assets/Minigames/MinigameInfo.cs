using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Minigame Info", menuName = "Minigames/MinigameInfo")]
public class MinigameInfo : ScriptableObject
{
    [Header("Info")]
    public string Name;
    [TextArea(3,2)] public string Description;
    [TextArea(3, 2)] public string HowToPlay;
    public Sprite DisplayImage;

    [Header("Scene")]
    public string MG_SceneName;

}
