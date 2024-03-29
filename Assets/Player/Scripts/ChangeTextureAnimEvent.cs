using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class AnimationPacks
{
    public string name;
    public Texture2D front;
    public Texture2D back;

    public AnimationPacks(string name)
    {
        this.name = name;
    }
}

[RequireComponent(typeof(Animator))]
public class ChangeTextureAnimEvent : MonoBehaviour
{
    public GameObject modelo;
    public int ID;
    public AnimationPacks[] texturePacks;

    Material frontMat, backMat;
    Dictionary<string, int> animationDic = new Dictionary<string, int>();

    private void Start()
    {   
        frontMat = modelo.GetComponent<MeshRenderer>().materials[0];
        backMat = modelo.GetComponent<MeshRenderer>().materials[1];

        //añade todas las texturas al diccionario
        animationDic.Clear();
        for (int i = 0; i < texturePacks.Length; i++)
        {
            animationDic.Add(texturePacks[i].name, i);
        }
    }

    public void ChangeTexture(string textureName)
    {
        //busca el respectivo pack de texturas segun lo que escribamos en el textureName
        if(animationDic.ContainsKey(textureName))
        {
            AnimationPacks textures = texturePacks[animationDic[textureName]];

            //cambia el albedo del material de adelante y atras
            frontMat.SetTexture("_MainTex", textures.front);
            backMat.SetTexture("_MainTex", textures.back);
        }
        else
        {
            Debug.LogWarning($"No se encontro la textura: {textureName}", this);
        }
    }
}