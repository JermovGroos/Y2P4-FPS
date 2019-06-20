using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Options : MonoBehaviour {

    //Resolution    
    bool fullscreen; //Fullscreen or not

    //Scenes
    [Header ("Scenes")]
    public string mainMenuName = "MainMenu";

    //Audio
    [Header ("Audio")]
    public AudioMixer audioMixer; //Main audio mixer
    public float defaultAudio = .75f; //Default audio scale
    public string audioSaveName = "MainVolume"; //Audio name under player prefs
    public string audioMixerName = "MainVolume"; //Volume parameter of mixer
    public Slider audioUISlider; //Slider that controls the audio

    void Start () {

        //Set audio levels at the beginning of the game
        audioUISlider.value = PlayerPrefs.GetFloat (audioSaveName, defaultAudio);
        SetAudioLevel (audioUISlider.value);
    }

    public void ToggleFullscreen () {

        //Make width and height variables
        int width = Screen.currentResolution.width;
        int height = Screen.currentResolution.height;

        //Toggle fullscreen
        if (fullscreen)
            Screen.SetResolution (Mathf.RoundToInt (width / 2), Mathf.RoundToInt (height / 2), false);
        else
            Screen.SetResolution (width, height, true);

        //Set variable back
        fullscreen = !fullscreen;
    }

    public void SetAudioLevel (float sliderValue) {

        //Create audio value float
        float audioValue;

        //Change audio mixer's values
        if (sliderValue > 0)
            audioValue = Mathf.Log10 (sliderValue) * 20;
        else
            audioValue = 0;

        //Set the values
        audioMixer.SetFloat (audioMixerName, audioValue);
        PlayerPrefs.SetFloat (audioSaveName, audioValue);
    }

    public void MainMenu () {

        //Load main menu
        print("Going to main menu...");
        SceneManager.LoadScene (mainMenuName);
    }

    public void QuitGame () {

        //Quit the game
        print ("Quitting...");
        Application.Quit();
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}