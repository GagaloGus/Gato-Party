using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerBox
{
    public GameObject Player;
    public GameObject Box;
}

public class FinalScoreAnimation : MonoBehaviour
{
    [Header("Player Objects")]
    [SerializeField] List<PlayerBox> PlayerBoxes = new List<PlayerBox>(4);

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(nameof(AnimationTimeLine));

        for (int i = 0; i < PlayerBoxes.Count; i++)
        {
            PlayerBox current = PlayerBoxes[i];

            current.Player.GetComponent<FinalScore_PlayerAnimator>().order = 4 - i;

            //Aqui ira todo sobre el cargado de skins
        }
    }

    IEnumerator AnimationTimeLine()
    {
        //Espera (suspense)
        yield return new WaitForSeconds(1.5f);

        //Recorre solo los 3 primeros players
        //Van cayendo poco a poco en orden inverso
        for (int i = 3; i >= 1; i--)
        {
            PlayerBoxes[i].Box.SetActive(false);
            print($"Cae el player {i}");

            yield return new WaitForSeconds(1f);

        }

        //Espera un poco y cae el primer player
        yield return new WaitForSeconds(0.7f);

        PlayerBoxes[0].Box.SetActive(false);
        print($"Cae el player {0}");

    }
}
