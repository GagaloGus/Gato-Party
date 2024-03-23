using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class CountdownController : MonoBehaviour
{
    [Header("References")]
    public GameObject counterGameObject;
    TMP_Text counterText;

    [Header("Time Variables")]
    private float counterValue = 0f;
    public float incrementAmount = 0.1f;
    public Action endCounterFunction;

    PhotonView photonView;

    private void Awake()
    {
        counterGameObject.SetActive(false);
        counterText = counterGameObject.GetComponentInChildren<TMP_Text>();
        photonView = GetComponent<PhotonView>();
    }

    public void StartCountdown(float maxTime, Action endCounterFunction)
    {
        counterGameObject.SetActive(true);
        this.endCounterFunction = endCounterFunction;
        counterValue = maxTime;

        if (PhotonNetwork.IsMasterClient)
        {
            // Si este cliente es el MasterClient, comienza el contador para que no se ejecuten varios a la vez
            photonView.RPC(nameof(RPC_StartCounter), RpcTarget.AllBuffered);
        }
    }


    [PunRPC]
    void RPC_StartCounter()
    {
        // Llama al método para iniciar el contador en todos los clientes
        InvokeRepeating(nameof(Countdown), 0, incrementAmount); // Baja el contador cada incremento repetidamente
    }

    void Countdown()
    {
        counterValue -= incrementAmount;
        UpdateCounterText();

        if(counterValue < 0f) 
        {
            CancelInvoke(nameof(Countdown));
            StopTimer();
        }
    }

    void UpdateCounterText()
    {
        // Formatea el valor del contador para mostrarlo en el texto
        counterText.text = $"Time: {counterValue.ToString("F1")}s";  // "F1" muestra un decimal
    }

    void StopTimer()
    {
        counterText.text = $"Starting minigame...";

        CoolFunctions.Invoke(this, () =>
        {
            print("Loading...");
            endCounterFunction();
        }, 2);
    }
}
