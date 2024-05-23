using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsHandler : MonoBehaviour
{
    [Header("References")]
    public Transform Panel;
    public Slider MusicSlider, SFXSlider;
    public TMP_Text MusicText, SFXText, UsernameText;

    float tmpVolumeSize = 50;
    AudioClip sfxSoundTest;

    private void Awake()
    {
        Panel = transform.Find("Panel");
        Transform Options = Panel.Find("Options");

        MusicSlider = Options.Find("MusicSlider").GetComponent<Slider>();
        SFXSlider = Options.Find("SFXSlider").GetComponent<Slider>();

        MusicText = Options.Find("MusicText").GetComponent<TMP_Text>();
        SFXText = Options.Find("SFXText").GetComponent<TMP_Text>();

        UsernameText = Options.Find("Username").GetComponent<TMP_Text>();

        Panel.gameObject.SetActive(false);

        //Si el objeto de guia no esta borrado del prefab, le cambie el texto. Si no existe no hace nada
        try
        {
            TMP_Text guide = transform.Find("OpenSettingsButton").Find("Guide").GetComponent<TMP_Text>();
            guide.text = $"You can press <b>{PlayerKeybinds.openSettings}</b> to toggle the settings";
        } catch { }

        sfxSoundTest = Resources.Load<AudioClip>("Sounds/Gato/Meow/gato sound_4");
    }

    // Start is called before the first frame update
    void Start()
    {
        MusicSlider.value = AudioManager.instance.volumeMusic;
        SFXSlider.value = AudioManager.instance.volumeSFX;

        UsernameText.text = $"Username: {PhotonNetwork.NickName}";
    }

    void Update()
    {
        MusicText.text = $"Music\n<size={tmpVolumeSize}>{(Mathf.Round(MusicSlider.value * 100))/100}";
        SFXText.text = $"SFX\n<size={tmpVolumeSize}>{(Mathf.Round(SFXSlider.value * 100)) / 100}";

        if(Input.GetKeyDown(PlayerKeybinds.openSettings) && PhotonNetwork.InRoom) 
        {
            Panel.gameObject.SetActive(!Panel.gameObject.activeInHierarchy);
        }
    }

    public void ChangeMusicVolume()
    {
        AudioManager.instance.SetMusicVolume(MusicSlider.value);
    }

    public void ChangeSFXVolume()
    {
        AudioManager.instance.SetSFXVolume(SFXSlider.value);
    }
}
