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
        print("Start countdown");
        counterGameObject.SetActive(true);

        this.endCounterFunction = endCounterFunction;
        this.finishedText = finishedText;
        this.incrementAmount = incrementAmount;
        this.stringFormat = stringFormat;
        this.delayToInvoke = delayToInvoke;
        counterValue = maxTime;

        object[] temp = { 
            maxTime,
            incrementAmount,
            finishedText,
            stringFormat,
            delayToInvoke,
        };

        if (PhotonNetwork.IsMasterClient)
        {
            // Si este cliente es el MasterClient, comienza el contador para que no se ejecuten varios a la vez
            photonView.RPC(nameof(RPC_StartCounter), RpcTarget.AllBuffered, temp);
        }
    }

    [PunRPC]
    void RPC_StartCounter(float maxTime, float incrementAmount, string finishedText, string stringFormat, float delayToInvoke)
    {
        print("Start RPC countdown");

        this.finishedText = finishedText;
        this.incrementAmount = incrementAmount;
        this.stringFormat = stringFormat;
        this.delayToInvoke = delayToInvoke;
        counterValue = maxTime;

        print($"Max time:{maxTime}, Increment: {incrementAmount}, Delay: {delayToInvoke}");

        // Llama al método para iniciar el contador en todos los clientes
        InvokeRepeating(nameof(Countdown), 0, incrementAmount); // Baja el contador cada incremento repetidamente
    }

    void Countdown()
    {
        counterValue -= incrementAmount; //Baja el contador
        UpdateCounterText(); //Actualiza la interfaz

        //Si el contador llega a 0
        if(Mathf.CeilToInt(counterValue) <= 0) 
        {
            //Deja de repetir el Invoke
            CancelInvoke(nameof(Countdown));
            //Para el contador
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
        //Depende si le pusimos algo en el finish text, lo escribe o escribe 0
        if (!string.IsNullOrEmpty(finishedText))
            counterText.text = finishedText;
        else
            counterText.text = 0.ToString(stringFormat);

        //Ejecuta la funcion que le pasamos
        CoolFunctions.Invoke(this, () =>
        {
            print("terminao");

            try { endCounterFunction();}
            catch { }

            //Reseta las variables
            endCounterFunction = null;
            finishedText = "";
            incrementAmount = 0;
            stringFormat = "F1";
            delayToInvoke = 0;
            counterValue = 0;

        }, delayToInvoke);
    }
}
