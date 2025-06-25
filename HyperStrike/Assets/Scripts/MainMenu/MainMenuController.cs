using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.UI;


public class MainMenuController : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioMixer mixer;
    
    [Space(10)]

    [SerializeField] private TMP_Text musicTextValue = null;
    [SerializeField] private Slider musicSlider = null;
    [SerializeField] private float defaultMusic = 0.5f;

    [Space(10)]

    [SerializeField] private TMP_Text sfxTextValue = null;
    [SerializeField] private Slider sfxSlider = null;
    [SerializeField] private float defaultSFX = 0.5f;

    [Header("Gameplay Settings")]
    [SerializeField] private TMP_Text sensitivityTextValue = null;
    [SerializeField] private Slider sensitivitySlider = null;
    [SerializeField] private int defaultSensitivity = 5;
    public int mainSensitivity = 5;

    [Header("Toggle Settings")]
    [SerializeField] private Toggle invertYToggle = null;

    [Header("Graphics Settings")]
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    private int _qualityLevel;
    private bool _isFullscreen;

    [Header("Resolution Dropdowns")]
    public TMP_Dropdown resolutionDropdown;
    private Resolution[] resolutions;


    [Header("Confirmation Icon")]
    [SerializeField] private GameObject confirmationPrompt = null;

    
    PlayerInput input;

    [Header("Pause")]
    [SerializeField] private GameObject pauseMenuContainer;
    [SerializeField] private bool canPause;
    private bool isPaused = false;

    private void Start()
    {
        input = new PlayerInput();
        if (!input.Player.enabled) input?.Player.Enable();
        if (canPause) input.Player.OpenClosePause.started += ctx => OpenClosePause();
        isPaused = false;

        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
  
    public void SetMusic(float music)
    {
        //AudioListener.volume = music;

        mixer.SetFloat("Music_Volume", Mathf.Log10(music) * 20);
        musicTextValue.text = music.ToString("0.0");
    }
    public void SetSFX(float sfx)
    {
        //AudioListener.volume = sfx;

        mixer.SetFloat("SFX_Volume", Mathf.Log10(sfx) * 20);
        sfxTextValue.text = sfx.ToString("0.0");
    }

    public void AudioApply()
    {
        PlayerPrefs.SetFloat("masterMusic", musicSlider.value);

        PlayerPrefs.SetFloat("masterSFX", sfxSlider.value);

        StartCoroutine(ConfirmationBox());
    }

    public void SetSensitivity(float sensitivity)
    {
        mainSensitivity = Mathf.RoundToInt(sensitivity);
        sensitivityTextValue.text = sensitivity.ToString("0");
    }

    public void GameplayApply() 
    {
        if(invertYToggle.isOn)
        {
            PlayerPrefs.SetInt("masterInvertY", 1);
            // invert y with Arnau's method
        }
        else
        {
            PlayerPrefs.SetInt("masterInvertY", 0);
            // not invert y with Arnau's method
        }

        PlayerPrefs.SetFloat("masterSen", mainSensitivity);
        // set sensitivity with Arnau's method

        StartCoroutine(ConfirmationBox());
    }

    public void SetFullscreen(bool isFullscreen)
    {
        _isFullscreen = isFullscreen;
    }

    public void SetQuality(int qualityIndex)
    {
        _qualityLevel = qualityIndex;
    }

    public void GraphicsApply()
    {
        PlayerPrefs.SetInt("masterQuality", _qualityLevel);
        QualitySettings.SetQualityLevel(_qualityLevel);

        PlayerPrefs.SetInt("masterFullscreen", (_isFullscreen ? 1 : 0));
        Screen.fullScreen = _isFullscreen;
        
        StartCoroutine(ConfirmationBox());
    }

    public void ResetButton(string MenuType)
    {
        if (MenuType == "Graphics")
        {
            qualityDropdown.value = 0;
            QualitySettings.SetQualityLevel(0);

            fullscreenToggle.isOn = false;
            Screen.fullScreen = false;

            Resolution currentResolution = Screen.currentResolution;
            Screen.SetResolution(currentResolution.width, currentResolution.height, Screen.fullScreen);
            resolutionDropdown.value = resolutions.Length;
            GraphicsApply();
        }

        if (MenuType == "Audio")
        {
            mixer.SetFloat("Music_Volume", Mathf.Log10(defaultMusic) * 20);
            musicSlider.value = defaultMusic;
            musicTextValue.text = defaultMusic.ToString("0.0");

            mixer.SetFloat("SFX_Volume", Mathf.Log10(defaultSFX) * 20);
            sfxSlider.value = defaultSFX;
            sfxTextValue.text = defaultSFX.ToString("0.0");

            AudioApply();
        }

        if (MenuType == "Gameplay")
        {
            sensitivityTextValue.text = defaultSensitivity.ToString("0");
            sensitivitySlider.value = defaultSensitivity;
            mainSensitivity = defaultSensitivity;
            invertYToggle.isOn = false;
            GameplayApply();
        }
    }

    public void ExitButton()
    {
        Application.Quit();
    }

    public IEnumerator ConfirmationBox()
    {
        confirmationPrompt.SetActive(true);
        yield return new WaitForSeconds(2);
        confirmationPrompt.SetActive(false);
    }

    public void OpenClosePause()
    {
        if (isPaused && pauseMenuContainer.activeSelf)
        {
            pauseMenuContainer.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            isPaused = false;
        }
        else if (!isPaused && !pauseMenuContainer.activeSelf)
        {
            pauseMenuContainer.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            isPaused = true;
        }
    }

    private void OnDestroy()
    {
        if (input != null && input.Player.enabled) input?.Player.Disable();
    }
}
