using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class LoadPrefs : MonoBehaviour
{
    [Header("General Setting")]
    [SerializeField] private bool canUse = false;
    [SerializeField] private MainMenuController mainMenuController;

    [Header("Audio Setting")]
    [SerializeField] private AudioMixer mixer;
    
    [Space(10)]

    [SerializeField] private TMP_Text musicTextValue = null;
    [SerializeField] private Slider musicSlider = null;
    
    [Space(10)]

    [SerializeField] private TMP_Text sfxTextValue = null;
    [SerializeField] private Slider sfxSlider = null;

    [Header("Quality Level Setting")]
    [SerializeField] private TMP_Dropdown qualityDropdown;

    [Header("Fullscreen Setting")]
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Sensitivity Setting")]
    [SerializeField] private TMP_Text sensitivityTextValue = null;
    [SerializeField] private Slider sensitivitySlider = null;

    [Header("Invert Y Setting")]
    [SerializeField] private Toggle invertYToggle = null;

    private void Start()
    {
        if (canUse)
        {
            // AUDIO MUSIC
            if (PlayerPrefs.HasKey("masterMusic"))
            {
                float localMusic = PlayerPrefs.GetFloat("masterMusic");

                musicTextValue.text = localMusic.ToString("0.0");
                musicSlider.value = localMusic;
                mixer.SetFloat("Music_Volume", Mathf.Log10(localMusic) * 20);
            }
            else
            {
                mainMenuController.ResetButton("Audio");
            }
            
            // AUDIO SFX
            if (PlayerPrefs.HasKey("masterSFX"))
            {
                float localSFX = PlayerPrefs.GetFloat("masterSFX");

                sfxTextValue.text = localSFX.ToString("0.0");
                sfxSlider.value = localSFX;
                mixer.SetFloat("SFX_Volume", Mathf.Log10(localSFX) * 20);
            }
            else
            {
                mainMenuController.ResetButton("Audio");
            }

            // QUALITY
            if (PlayerPrefs.HasKey("masterQuality"))
            {
                int localQuality = PlayerPrefs.GetInt("masterQuality");
                qualityDropdown.value = localQuality;
                QualitySettings.SetQualityLevel(localQuality);
            }

            // FULLSCREEN
            if (PlayerPrefs.HasKey("masterFullscreen"))
            {
                int localFullscreen = PlayerPrefs.GetInt("masterFullscreen");

                if(localFullscreen == 1)
                {
                    Screen.fullScreen = true;
                    fullscreenToggle.isOn = true;
                }
                else
                {
                    Screen.fullScreen = false;
                    fullscreenToggle.isOn = false; 
                }
            }

            // SENSITIVITY
            if (PlayerPrefs.HasKey("masterSen"))
            {
                float localSensitivity = PlayerPrefs.GetFloat("masterSen");

                sensitivityTextValue.text = localSensitivity.ToString("0");
                sensitivitySlider.value = localSensitivity;
                mainMenuController.mainSensitivity = Mathf.RoundToInt(localSensitivity);
            }
            else
            {
                mainMenuController.ResetButton("Gameplay");
            }

            // MOUSE INVERT Y
            if (PlayerPrefs.HasKey("masterInvertY"))
            {
                if (PlayerPrefs.GetInt("masterInvertY") == 1)
                {
                    invertYToggle.isOn = true;
                }
                else
                {
                    invertYToggle.isOn = false;
                }
            }

        }
    }
}
