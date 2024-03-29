using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class SkinSelector : MonoBehaviourPunCallbacks
{
    AnimationBundles animationBundles;
    private void Awake()
    {
        animationBundles = GetComponent<AnimationBundles>();
    }

    // Start is called before the first frame update
    public int GetSkinIDAvaliable()
    {
        //Recorre todos los players y guarda sus IDs de sus skins
        List<int> skinList = new List<int>();
        foreach (KeyValuePair<int, Player> player in PhotonNetwork.CurrentRoom.Players)
        {
            Player currentPlayer = player.Value;
            foreach (System.Collections.DictionaryEntry entry in currentPlayer.CustomProperties)
            {
                if ((string)entry.Key == Constantes.PlayerKey_Skin)
                {
                    skinList.Add((int)currentPlayer.CustomProperties[Constantes.PlayerKey_Skin]);
                    break;
                }
            }
        }

        //Ordena la lista de menor a mayor
        skinList.Sort();

        //ID mas bajo por default es 0
        int avaliableSkin = 0;

        //Si los IDs coinciden, se le suma 1
        foreach(int skinID in skinList)
        {
            if(skinID == avaliableSkin)
            {
                avaliableSkin++;
            }
        }

        return avaliableSkin;
    }
}
