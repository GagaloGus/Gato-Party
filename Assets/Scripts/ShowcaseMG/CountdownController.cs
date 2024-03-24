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
    [SerializeField] TMP_Text counterText;

    [Header("Time Variables")]
    float counterValue = 0f;
    string finishedText;
    string stringFormat = "F1";
    float incrementAmount = 1;
    float delayToInvoke;
    public Action endCounterFunction;

    PhotonView photonView;

    private void Awake()
    {
        counterText = counterGameObject.GetComponentInChildren<TMP_Text>();
        counterGameObject.SetActive(false);
        photonView = GetComponent<PhotonView>();
    }

    public void StartCountdown(float maxTime, Action endCounterFunction, float incrementAmount = 1, string finishedText = "", string stringFormat = "0", float delayToInvoke = 0)
    {
        counterGameObject.SetActive(true);

        this.endCounterFunction = endCounterFunction;
        this.finishedText = finishedText;
        this.incrementAmount = incrementAmount;
        this.stringFormat = stringFormat;
        this.delayToInvoke = delayToInvoke;
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
        print(counterValue);

        if(Mathf.CeilToInt(counterValue) <= 0) 
        {
            CancelInvoke(nameof(Countdown));
            StopTimer();
        }
    }

    void UpdateCounterText()
    {
        // Formatea el valor del contador para mostrarlo en el texto
        counterText.text = (counterValue).ToString(stringFormat);
    }

    void StopTimer()
    {
        if (!string.IsNullOrEmpty(finishedText))
            counterText.text = finishedText;
        else
            counterText.text = 0.ToString(stringFormat);

        CoolFunctions.Invoke(this, () =>
        {
            print("terminao");

            try { endCounterFunction();}
            catch { }

        }, delayToInvoke);
    }
}
