using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{

    #region Singleton

    public static UIManager instance;

    public static UIManager Instance { get { return instance; } }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }

    #endregion

    public CharacterController charCon;

    public GameObject mainMenuPanel;

    public GameObject optionsMenuPanel;
    //---
    public GameObject optionsVideoPanel;
    public TMP_Dropdown resolutionDropdown;
    Resolution[] resolutions;
    //-
    public GameObject optionsAudioPanel;
    public AudioMixer audioMixer;
    Slider[] audioSlider;
    //-
    public GameObject optionsControlsPanel;
    //---
    public GameObject loadingScreenPanel;
    Slider loadingSlider;
    TMP_Text loadingText;
    //---
    public GameObject inGamePanel;
    Slider healthSlider;
    Image[] images;


    // Start is called before the first frame update
    void Start()
    {
    //    charCon = GameManager.instance.player.GetComponent<CharacterController>();

        healthSlider = inGamePanel.GetComponentInChildren<Slider>();
        // image 0-3 fuer costumes, 4-6 fuer abilities
        images = inGamePanel.GetComponentsInChildren<Image>();

        loadingText = loadingScreenPanel.GetComponentInChildren<TMP_Text>();
        loadingSlider = loadingScreenPanel.GetComponentInChildren<Slider>();

        audioSlider = optionsAudioPanel.GetComponentsInChildren<Slider>();

        Resolutions();

        onUIchanged += UpdateHealth;
        onUIchanged += UpdateImages;
    }

    #region InGameScreen
    public delegate void OnUpdateUI();
    public OnUpdateUI onUIchanged;

    public void UpdateHealth()
    {
        healthSlider.value = charCon.curHealth / charCon.maxHealth;
    }

    public void UpdateImages()
    {
        images[0].sprite = charCon.activeCostume.sprite;

        // Kostüme
        for (int i = 1; i <= 3; i++)
        {
            images[i].sprite = charCon.costumes[i].sprite;
        }
        // Abilities
        for (int i = 4; i <= 6; i++)
        {
            images[i].sprite = charCon.activeAbilities[i-4].sprite;
        }
    }

    public IEnumerator UpdateAbilityCD(Ability usedAbility)
    {
        for (int i = 4; i < 6; i++)
        {
            if (charCon.activeAbilities[i-4] == usedAbility)
            {
                float tempDuration = usedAbility.duration;
                while (tempDuration >= 0f - Time.deltaTime)
                {
                    images[i].fillAmount = usedAbility.duration - tempDuration;
               //     Debug.Log("Still counting :" + tempDuration);
                    tempDuration -= Time.deltaTime;
                    yield return new WaitForSeconds(Time.deltaTime);
                }
            }
        }
    }

    #endregion

    #region LoadingScreen
    //
    // Loading Screen Funktion und Coroutine
    //
    AsyncOperation async;

    public void RunLoadingScreen(string sceneName)
    {
        StartCoroutine(LoadingScreen(sceneName));
    }

    IEnumerator LoadingScreen(string sceneName)
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);
        async.allowSceneActivation = false;

        while (async.progress < 0.9f)
        {
            loadingScreenPanel.SetActive(true);
            var scaledPerc = 0.5f * async.progress / 0.9f;
            loadingText.text = "<Loading Map : " + sceneName.ToString() + " : " + (100f * scaledPerc).ToString("F0") + ">";
        }

        async.allowSceneActivation = true;
        float perc = 0.5f;
        while (!async.isDone)
        {
            yield return null;
            perc = Mathf.Lerp(perc, 1f, 0.05f);
            loadingText.text = "<Loading Map : " + sceneName.ToString() + " : " + (100f * perc).ToString("F0") + ">";
        }

        loadingText.text = "<Loading Complete : " + sceneName.ToString() + " : 100>";
        loadingScreenPanel.SetActive(false);

        // Deaktiviere Master Canvas in Game !
        mainMenuPanel.gameObject.SetActive(false);
        inGamePanel.gameObject.SetActive(true);
    }

    #endregion

    #region MainMenu

    public void LoadInGameScene()
    {
        RunLoadingScreen("GameManagerScene");
    }

    public void LoadOptionsMenu()
    {
        optionsMenuPanel.gameObject.SetActive(!optionsMenuPanel.gameObject.activeSelf);
    }

    #endregion

    #region OptionsMenu

    public void Resolutions()
    {
        resolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void OptionsBackButton()
    {
        optionsMenuPanel.gameObject.SetActive(false);
    }

    #region AudioSettings
    public void OptionsAudioButton()
    {
        if (optionsVideoPanel.activeSelf)
        {
            optionsVideoPanel.SetActive(false);
        }
        if (optionsControlsPanel.activeSelf)
        {
            optionsControlsPanel.SetActive(false);
        }
        optionsAudioPanel.gameObject.SetActive(true);
    }

    public void ChangeMasterVolume(float volume)
    {
        volume = audioSlider[0].value;
        audioMixer.SetFloat("MasterVolume", volume);
    }

    public void ChangeMusicVolume(float volume)
    {
        volume = audioSlider[1].value;
        audioMixer.SetFloat("MusicVolume", volume);
    }
    public void ChangeSoundVolume(float volume)
    {
        volume = audioSlider[2].value;
        audioMixer.SetFloat("SoundVolume", volume);
    }
    #endregion

    #region VideoSettings
    public void OptionsVideoButton()
    {
        if (optionsAudioPanel.activeSelf)
        {
            optionsAudioPanel.SetActive(false);
        }
        if (optionsControlsPanel.activeSelf)
        {
            optionsControlsPanel.SetActive(false);
        }
        optionsVideoPanel.gameObject.SetActive(!optionsVideoPanel.gameObject.activeSelf);
    }

    public void ChangeResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void ChangeGraphicsPreset(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void ChangeFullscreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }

    #endregion

    #region ControlSettings
    public void OptionsControlsButton()
    {
        if (optionsAudioPanel.activeSelf)
        {
            optionsAudioPanel.SetActive(false);
        }
        if (optionsVideoPanel.activeSelf)
        {
            optionsVideoPanel.SetActive(false);
        }
        optionsControlsPanel.gameObject.SetActive(!optionsControlsPanel.gameObject.activeSelf);
    }

    #endregion

    #endregion

    // Update is called once per frame
    void Update()
    {

    }
}
