using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constantes
{
    //Players
    public static readonly string PlayerKey_CustomID = "PLAYER_CUSTOM_ID";
    public static readonly string PlayerKey_Skin = "PLAYER_SKIN";

    public static readonly string PlayerKey_Ready_SalaEspera = "PLAYER_READY_SALAESPERA";
    public static readonly string PlayerKey_Ready_SMG = "PLAYER_READY_SMG";

    public static readonly string PlayerKey_TotalScore = "PLAYER_TOTALSCORE";
    public static readonly string PlayerKey_MinigameScore = "PLAYER_MGSCORE";
    public static readonly string PlayerKey_Eliminated = "PLAYER_ELIMINATED";

    //Rooms
    public static readonly string MinigameOrder_Room = "MINIGAME_ORDER";
    public static readonly string MinigameScene_Room = "MINIGAME_SCENE";
    public static readonly string SkinIDOrder_Room = "SKIN_ID_ORDER";
    public static readonly string RoundsOver_Room = "ROUNDSOVER";

    //Minijuegos
    public static readonly string KeyOrder_MGCommand = "KEY_ORDER";

    //Puntos
    public static readonly int[] Win_Points = new int[] { 7, 4, 2, 0 };
}
