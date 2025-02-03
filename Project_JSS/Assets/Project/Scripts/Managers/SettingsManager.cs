using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using System.Collections.Generic;

public class SettingsManager : BaseManager
{
    #region SerializeField Variables
    [Header("UI Canvases")]
    [SerializeField] private Canvas settingCanvas;
    [SerializeField] private GameObject soundSetting;
    [SerializeField] private GameObject gameSetting;
    [SerializeField] private RectTransform arrow;

    [Header("Buttons")]
    [SerializeField] private Button soundTabButton;
    [SerializeField] private Button gameTabButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button mainButton;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource audioSourceBGM;
    [SerializeField] private AudioSource audioSourceUI;
    [SerializeField] private AudioSource audioSourceEffect;

    [Header("Volume UI")]
    [SerializeField] private TextMeshProUGUI masterVolumeText;
    [SerializeField] private TextMeshProUGUI bgmVolumeText;
    [SerializeField] private TextMeshProUGUI UIVolumeText;
    [SerializeField] private TextMeshProUGUI effectVolumeText;

    [Header("Game Settings UI")]
    [SerializeField] private TextMeshProUGUI screenModeText;
    [SerializeField] private TextMeshProUGUI resolutionText;
    [SerializeField] private TextMeshProUGUI talkSpeedText;
    [SerializeField] private TextMeshProUGUI talkSpeedText_2;
    [SerializeField] private TextMeshProUGUI languageText;
    [SerializeField] private Button[] languageButtons = new Button[2];
    #endregion

    #region Private Variables
    // Volume Settings
    public float masterVolume = 0.5f;
    public float bgmVolume = 0.5f;
    
    public float uiVolume = 0.5f;
    public float effectVolume = 0.5f;
    private const float VolumeStep = 0.05f;

    // Screen Settings
    private bool isFullScreen = true;
    private Resolution[] customResolutions;  // 변경된 부분
    //private Resolution[] resolutions;
    private int currentResolutionIndex = 0;

    // Talk Speed Settings
    private bool isFast;
    private float[] typingSpeedList = new float[3] { 0.08f, 0.05f, 0.03f };
    private int typingSpeedIndex = 1;
    public float typingSpeed;

    private bool isFast_2;
    public float isFast_2_float;
    private float[] bubbleSpeedList = new float[3] { 0.75f, 0.5f, 0.25f};
    private int bubbleSpeedIndex = 1;

    //private const float normalTypingSpeed = 0.05f;
    //private const float fastTypingSpeed = 0.03f;

    // State Variables
    private bool isInPub = false;
    private bool checkIsDialogueActive = false;
    public bool isOnLog = false;
    public bool isOnMap = false;

    // Language Settings
    [SerializeField] private int currentLanguageIndex = 0;
    #endregion

    #region PlayerPrefs Keys
    // Volume Keys
    private const string MasterVolumeKey = "MasterVolume";
    private const string BGMVolumeKey = "BGMVolume";
    private const string UIVolumeKey = "UIVolume";
    private const string EffectVolumeKey = "EffectVolume";

    // Screen Settings Keys
    private const string ScreenModeKey = "ScreenMode";
    private const string ResolutionWidthKey = "Resolution";
    //private const string ResolutionHeightKey = "ResolutionHeight";

    // Talk Speed Keys
    private const string TalkSpeedKey = "TalkSpeed";
    private const string TalkSpeed2Key = "TalkSpeed2";

    // Language Key
    private const string LanguageKey = "SelectedLanguage";
    #endregion

    #region String Table Keys
    private static class LocalizationKeys
    {
        // Settings 테이블의 키값들
        public const string FULLSCREEN = "FULLSCREEN";
        public const string WINDOWED = "WINDOWED";

        public const string SLOW_SPEED = "SLOW_SPEED";
        public const string NORMAL_SPEED = "NORMAL_SPEED";
        public const string FAST_SPEED = "FAST_SPEED";

        // Speed array
        public static readonly string[] SpeedKeys = new string[]
        {
            SLOW_SPEED,
            NORMAL_SPEED,
            FAST_SPEED
        };

        // 테이블 이름
        public const string SETTINGS_TABLE = "SETTINGS";
        
        // 전체 경로 (테이블/키)
        public static string GetTableKey(string key) => $"{SETTINGS_TABLE}/{key}";
    }
    #endregion

    #region Unity Methods
    //빌드파일에서만 처음실행시 PlayerPrefs 초기화
    private void Awake()
    {
        #if !UNITY_EDITOR // 빌드된 버전에서만 실행
        if (!PlayerPrefs.HasKey("FirstLaunch"))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.SetInt("FirstLaunch", 1);
            PlayerPrefs.Save();
        }
        #endif
    }
    public void Start()
    {
        SetupButtonListeners();
        UpdateAllVolumes();

    }
    public float GetCurrentBGMVolume()
    {
        return bgmVolume*masterVolume;
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            HandleEscapeKey();
        }
        //if (Input.GetKeyDown(KeyCode.F11))
        //{
        //    QuitGame();
        //}
        //if (Input.GetKeyDown(KeyCode.F1))
        //{
        //    DataSave.Instance.ChapterNumber = 0;
        //    SceneManager.LoadScene("Test_TaeSeong");
        //}
        //if (Input.GetKeyDown(KeyCode.F2))
        //{
        //    DataSave.Instance.ChapterNumber = 1;
        //    SceneManager.LoadScene("Test_TaeSeong");
        //}
        //if (Input.GetKeyDown(KeyCode.F3))
        //{
        //    DataSave.Instance.ChapterNumber = 2;
        //    SceneManager.LoadScene("Test_TaeSeong");
        //}
        //if (Input.GetKeyDown(KeyCode.F4))
        //{
        //    DataSave.Instance.ChapterNumber = 2;
        //    SceneManager.LoadScene("Test_Wanil");
        //}
    }
    #endregion
    #region Initialization
    public override void Init()
    {
        base.Init();
        InitializeComponents();
        InitializeResolutions(); // 해상도 초기화 추가
        LoadAllSettings();
        //기본 영어(2)로 설정
        //currentLanguageIndex = PlayerPrefs.GetInt(LanguageKey, 2);
        StartCoroutine(InitializeLocalization());

    }

    private void InitializeComponents()
    {
        // 카메라 설정
        settingCanvas.worldCamera = Camera.main;

        // 오디오 소스 설정
        audioSourceBGM = Managers.Asset.audioSourceBGM;
        audioSourceUI = Managers.Asset.audioSourceUI;
        audioSourceEffect = Managers.Asset.audioSourceEffect;

        // //타이틀씬이 아닐 경우 언어 설정 비활성화
        // if (!SceneManager.GetActiveScene().name.Contains("TitleScene2"))
        // {
        //     if (languageButtons[0] != null)
        //     {
        //         languageButtons[0].interactable = false;
        //     }
        //     if (languageButtons[1] != null)
        //     {
        //         languageButtons[1].interactable = false;
        //     }
        // }
        // else
        // {
        //     if (languageButtons[0] != null)
        //     {
        //         languageButtons[0].interactable = true;
        //     }
        //     if (languageButtons[1] != null)
        //     {
        //         languageButtons[1].interactable = true;
        //     }
        // }
    }



    private void InitializeResolutions()
    {
        // resolutions = Screen.resolutions;
        // currentResolutionIndex = GetCurrentResolutionIndex();
        customResolutions = new Resolution[]
        {
            CreateResolution(1280, 720),   // HD
            CreateResolution(1600, 900),   // HD+
            CreateResolution(1920, 1080),  // Full HD
            CreateResolution(2560, 1440)  // QHD
            //CreateResolution(3840, 2160)   // 4K UHD
        };
        
        // 현재 화면 해상도와 가장 가까운 해상도 찾기
        currentResolutionIndex = GetNearestResolutionIndex(Screen.currentResolution.width);

    }
    private Resolution CreateResolution(int width, int height)
    {
        Resolution res = new Resolution();
        res.width = width;
        res.height = height;
        return res;
    }

    private int GetNearestResolutionIndex(int currentWidth)
    {
        int nearestIndex = 0;
        int minDiff = int.MaxValue;
        
        for (int i = 0; i < customResolutions.Length; i++)
        {
            int diff = Mathf.Abs(customResolutions[i].width - currentWidth);
            if (diff < minDiff)
            {
                minDiff = diff;
                nearestIndex = i;
            }
        }
        return nearestIndex;
    }


    #endregion
    #region Settings Load/Save
    private void LoadAllSettings()
    {
        // 화면 설정 불러오기
        isFullScreen = PlayerPrefs.GetInt(ScreenModeKey, 1) == 1;
        Screen.fullScreen = isFullScreen;
        screenModeText.GetComponent<LocalizeStringEvent>().StringReference.TableEntryReference
            = isFullScreen ? LocalizationKeys.FULLSCREEN : LocalizationKeys.WINDOWED;

        // 해상도 설정 불러오기
        //int savedWidth = PlayerPrefs.GetInt(ResolutionWidthKey, Screen.currentResolution.width);
        //int savedHeight = PlayerPrefs.GetInt(ResolutionHeightKey, Screen.currentResolution.height);
        int savedResolutionIndex = PlayerPrefs.GetInt(ResolutionWidthKey, 2);  // 기본값 1920x1080 (인덱스 2)
    
        //Debug.Log(savedWidth + " " + savedHeight);
        LoadResolutionSettings(savedResolutionIndex);
        // 대화 속도 설정 불러오기
        LoadTalkSpeedSettings();
        // 볼륨 설정 불러오기
        LoadVolumes();
        UpdateAllVolumes();
        // 탭 설정 초기화 (UI 초기 상태 설정)
        InitializeTabUI();
    }

    // UI 초기 상태 설정을 위한 새로운 메서드
    private void InitializeTabUI()
    {
        soundSetting.SetActive(false);
        soundTabButton.interactable = true;
        soundTabButton.transform.GetChild(0).gameObject.SetActive(true);
        soundTabButton.transform.GetChild(1).gameObject.SetActive(false);

        gameSetting.SetActive(true);
        gameTabButton.interactable = false;
        gameTabButton.transform.GetChild(0).gameObject.SetActive(false);
        gameTabButton.transform.GetChild(1).gameObject.SetActive(true);

        RectTransform gameTabButtonRectTr = gameTabButton.GetComponent<RectTransform>();
        Vector2 gameTabPosition = gameTabButtonRectTr.anchoredPosition;
        arrow.anchoredPosition = new Vector2(-230f, gameTabPosition.y);
    }

    // LoadResolutionSettings()에서
    private void LoadResolutionSettings(int savedIndex)
    {
        if (!PlayerPrefs.HasKey(ResolutionWidthKey))
        {
            // 기본 해상도를 1920x1080으로 설정 (인덱스 2)
            currentResolutionIndex = 2;
        }
        else 
        {
            currentResolutionIndex = savedIndex;
        }
        
        Screen.SetResolution(
            customResolutions[currentResolutionIndex].width, 
            customResolutions[currentResolutionIndex].height, 
            isFullScreen
        );
        UpdateResolutionText();
    }

    private void LoadTalkSpeedSettings()
    {
        if (!PlayerPrefs.HasKey(TalkSpeedKey))
        {
            // 기본 대화 속도 설정
            typingSpeedIndex = 1;
            bubbleSpeedIndex = 1;
            typingSpeed = typingSpeedList[typingSpeedIndex];
            isFast_2_float = bubbleSpeedList[bubbleSpeedIndex];
            
            PlayerPrefs.SetInt(TalkSpeedKey, typingSpeedIndex);
            PlayerPrefs.SetInt(TalkSpeed2Key, bubbleSpeedIndex);
        }
        else 
        {
            typingSpeedIndex = PlayerPrefs.GetInt(TalkSpeedKey);
            bubbleSpeedIndex = PlayerPrefs.GetInt(TalkSpeed2Key);
            typingSpeed = typingSpeedList[typingSpeedIndex];
            isFast_2_float = bubbleSpeedList[bubbleSpeedIndex];
        }

        talkSpeedText.GetComponent<LocalizeStringEvent>().StringReference.TableEntryReference
            = LocalizationKeys.SpeedKeys[typingSpeedIndex];
        talkSpeedText_2.GetComponent<LocalizeStringEvent>().StringReference.TableEntryReference
            = LocalizationKeys.SpeedKeys[bubbleSpeedIndex];
    }

    private void LoadVolumes()
    {
        // 볼륨이 한 번도 저장된 적 없다면 기본값 저장
        if (!PlayerPrefs.HasKey(MasterVolumeKey))
        {
            masterVolume = 0.5f;
            bgmVolume = 0.5f;
            uiVolume = 0.5f;
            effectVolume = 0.5f;
            SaveVolumes();
        }
        else 
        {
            masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey);
            bgmVolume = PlayerPrefs.GetFloat(BGMVolumeKey);
            uiVolume = PlayerPrefs.GetFloat(UIVolumeKey);
            effectVolume = PlayerPrefs.GetFloat(EffectVolumeKey);
        }
    }

    private void SaveAllSettings()
    {
        // 화면 설정 저장
        PlayerPrefs.SetInt(ScreenModeKey, isFullScreen ? 1 : 0);
        //PlayerPrefs.SetInt(ResolutionWidthKey, customResolutions[currentResolutionIndex].width);
        //PlayerPrefs.SetInt(ResolutionHeightKey, customResolutions[currentResolutionIndex].height);
        PlayerPrefs.SetInt(ResolutionWidthKey, currentResolutionIndex);  // 너비/높이 대신 인덱스만 저장
        // 대화 속도 설정 저장
        PlayerPrefs.SetInt(TalkSpeedKey, typingSpeedIndex);
        PlayerPrefs.SetInt(TalkSpeed2Key, bubbleSpeedIndex);

        // 볼륨 설정 저장
        SaveVolumes();

        PlayerPrefs.Save();
    }

    private void SaveVolumes()
    {
        PlayerPrefs.SetFloat(MasterVolumeKey, masterVolume);
        PlayerPrefs.SetFloat(BGMVolumeKey, bgmVolume);
        PlayerPrefs.SetFloat(UIVolumeKey, uiVolume);
        PlayerPrefs.SetFloat(EffectVolumeKey, effectVolume);
    }
    #endregion
    #region UI Event Handlers
    private void SetupButtonListeners()
    {
        soundTabButton.onClick.AddListener(OnSoundTabButtonPressed);
        gameTabButton.onClick.AddListener(OnGameTabButtonPressed);
        closeButton.onClick.AddListener(CloseSettingCanvas);
        quitButton.onClick.AddListener(QuitGame);
        mainButton.onClick.AddListener(GoToMain);
    }

    private void HandleEscapeKey()
    {
        if (settingCanvas.enabled)
        {
            CloseSettingCanvas();
        }
        else
        {
            OpenSettingCanvas();
        }
    }

    public void OpenSettingCanvas()
    {
        Managers.Asset.PlaySound("Popup", SoundType.UI);
        settingCanvas.enabled = true;
    }
    public void CloseSettingCanvas()
    {
        SaveAllSettings();
        Managers.Asset.PlaySound("Popup", SoundType.UI);
        settingCanvas.enabled = false;
    }
    public void QuitGame()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }

    public void GoToMain()
    {
        SceneManager.LoadScene("TitleScene2");
    }
    public void OnSoundTabButtonPressed()
    {
        Managers.Asset.PlaySound("Click", SoundType.UI);
        soundSetting.SetActive(true);
        soundTabButton.interactable = false;
        soundTabButton.transform.GetChild(0).gameObject.SetActive(false);
        soundTabButton.transform.GetChild(1).gameObject.SetActive(true);
        gameSetting.SetActive(false);
        gameTabButton.interactable = true;
        gameTabButton.transform.GetChild(0).gameObject.SetActive(true);
        gameTabButton.transform.GetChild(1).gameObject.SetActive(false);
        RectTransform soundTabButtonRectTr = soundTabButton.GetComponent<RectTransform>();
        soundTabButtonRectTr.DOKill();
        soundTabButtonRectTr.localScale = Vector3.one;
        Vector2 soundTabPosition = soundTabButtonRectTr.anchoredPosition;
        arrow.anchoredPosition = new Vector2(-280f, soundTabPosition.y);
    }
    public void OnGameTabButtonPressed()
    {
        Managers.Asset.PlaySound("Click", SoundType.UI);
        soundSetting.SetActive(false);
        soundTabButton.interactable = true;
        soundTabButton.transform.GetChild(0).gameObject.SetActive(true);
        soundTabButton.transform.GetChild(1).gameObject.SetActive(false);
        gameSetting.SetActive(true);
        gameTabButton.interactable = false;
        gameTabButton.transform.GetChild(0).gameObject.SetActive(false);
        gameTabButton.transform.GetChild(1).gameObject.SetActive(true);
        RectTransform gameTabButtonRectTr = gameTabButton.GetComponent<RectTransform>();
        gameTabButtonRectTr.DOKill();
        gameTabButtonRectTr.localScale = Vector3.one;
        Vector2 gameTabPosition = gameTabButtonRectTr.anchoredPosition;
        arrow.anchoredPosition = new Vector2(-230f, gameTabPosition.y);
    }
    #endregion
    #region Settings Controls
    #region Volume Controls
    public void OnMasterVolumeButtonPressed(bool increase)
    {
        Managers.Asset.PlaySound("Click", SoundType.UI);
        masterVolume = Mathf.Clamp01(masterVolume + (increase ? VolumeStep : -VolumeStep));
        Debug.Log($"master : {masterVolume}");
        UpdateAllVolumes();
        SaveVolumes();
    }

    public void OnBGMVolumeButtonPressed(bool increase)
    {
        Managers.Asset.PlaySound("Click", SoundType.UI);
        bgmVolume = Mathf.Clamp01(bgmVolume + (increase ? VolumeStep : -VolumeStep));
        UpdateBGMVolume();
        SaveVolumes();
    }

    public void OnUIVolumeButtonPressed(bool increase)
    {
        Managers.Asset.PlaySound("Click", SoundType.UI);
        uiVolume = Mathf.Clamp01(uiVolume + (increase ? VolumeStep : -VolumeStep));
        UpdateUIVolume();
        SaveVolumes();
    }

    public void OnEffectVolumeButtonPressed(bool increase)
    {
        Managers.Asset.PlaySound("Click", SoundType.UI);
        effectVolume = Mathf.Clamp01(effectVolume + (increase ? VolumeStep : -VolumeStep));
        UpdateEffectVolume();
        SaveVolumes();
    }

    private void UpdateAllVolumes()
    {
        UpdateBGMVolume();
        UpdateUIVolume();
        UpdateEffectVolume();
        masterVolumeText.text = $"{Mathf.RoundToInt(masterVolume * 100)}%";
    }

    private void UpdateBGMVolume()
    {
        audioSourceBGM.volume = masterVolume * bgmVolume;
        bgmVolumeText.text = $"{Mathf.RoundToInt(bgmVolume * 100)}%";
    }

    private void UpdateUIVolume()
    {
        audioSourceUI.volume = masterVolume * uiVolume;
        UIVolumeText.text = $"{Mathf.RoundToInt(uiVolume * 100)}%";
    }

    private void UpdateEffectVolume()
    {
        audioSourceEffect.volume = masterVolume * effectVolume;
        effectVolumeText.text = $"{Mathf.RoundToInt(effectVolume * 100)}%";
    }
    #endregion

    #region Screen Controls
    public void OnScreenModeButtonPressed()
    {
        Managers.Asset.PlaySound("Click", SoundType.UI);
        isFullScreen = !isFullScreen;
        Screen.fullScreen = isFullScreen;

        screenModeText.GetComponent<LocalizeStringEvent>().StringReference.TableEntryReference
            = isFullScreen ? "FULLSCREEN" : "WINDOWED";

        SaveAllSettings();
    }

    public void OnResolutionButtonPressed(bool next)
    {
        Managers.Asset.PlaySound("Click", SoundType.UI);

        // if (next)
        // {
        //     currentResolutionIndex = (currentResolutionIndex + 1) % resolutions.Length;
        // }
        // else
        // {
        //     currentResolutionIndex--;
        //     if (currentResolutionIndex < 0) currentResolutionIndex = resolutions.Length - 1;
        // }

        if (next)
        {
            currentResolutionIndex = (currentResolutionIndex + 1) % customResolutions.Length;
        }
        else
        {
            currentResolutionIndex--;
            if (currentResolutionIndex < 0) currentResolutionIndex = customResolutions.Length - 1;
        }

        StartCoroutine(ApplyAndSaveResolution());
    }

    private IEnumerator ApplyAndSaveResolution()
    {
        Resolution resolution = customResolutions[currentResolutionIndex];
        //Resolution resolution = resolutions[currentResolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        yield return null;
        UpdateResolutionText();
        SaveAllSettings();


    }

    private void UpdateResolutionText()
    {
        //Resolution resolution = resolutions[currentResolutionIndex];
        Resolution resolution = customResolutions[currentResolutionIndex];
        resolutionText.text = $"{resolution.width} x {resolution.height}";
    }

    private int GetCurrentResolutionIndex()
    {
        // Resolution currentResolution = Screen.currentResolution;
        // for (int i = 0; i < resolutions.Length; i++)
        // {
        //     if (resolutions[i].width == currentResolution.width &&
        //         resolutions[i].height == currentResolution.height)
        //     {
        //         return i;
        //     }
        // }
        // return 0;
        return GetNearestResolutionIndex(Screen.currentResolution.width);

    }
    #endregion

    #region Talk Speed Controls

    public void OnTalkSpeedButtonPressed(bool left)
    {
        //if (!isInPub) return;

        Managers.Asset.PlaySound("Click", SoundType.UI);

        if(left && typingSpeedIndex>0)
        {
            typingSpeedIndex -= 1;
        }

        if (!left && typingSpeedIndex < typingSpeedList.Length-1)
        {
            typingSpeedIndex += 1;
        }
        typingSpeed = typingSpeedList[typingSpeedIndex];
        talkSpeedText.GetComponent<LocalizeStringEvent>().StringReference.TableEntryReference
            = LocalizationKeys.SpeedKeys[typingSpeedIndex];

        SaveAllSettings();
    }

    public void OnTalkSpeedButtonPressed2(bool left)
    {
        Managers.Asset.PlaySound("Click", SoundType.UI);

        if (left && bubbleSpeedIndex > 0)
        {
            bubbleSpeedIndex -= 1;
        }

        if (!left && bubbleSpeedIndex < bubbleSpeedList.Length - 1)
        {
            bubbleSpeedIndex += 1;
        }
        isFast_2_float = bubbleSpeedList[bubbleSpeedIndex];
        talkSpeedText_2.GetComponent<LocalizeStringEvent>().StringReference.TableEntryReference
            = LocalizationKeys.SpeedKeys[bubbleSpeedIndex];

        SaveAllSettings();
    }
    #endregion

    #region Language Controls
    private IEnumerator InitializeLocalization()
    {
        // 한국어와 영어만 사용가능할때
        // yield return LocalizationSettings.InitializationOperation;
        // // 초기 언어 인덱스를 0(한국어)과 1(영어) 사이로 제한
        // currentLanguageIndex = Mathf.Clamp(currentLanguageIndex, 0, 1);
        // ChangeLanguage(currentLanguageIndex);
        // UpdateLanguageText();


        //한국어,영어,중국 사용 가능 및 초기 언어 중국어로 설정
        yield return LocalizationSettings.InitializationOperation;
        // 초기 언어 인덱스를 3(중국어)으로 설정
        currentLanguageIndex = PlayerPrefs.GetInt(LanguageKey, 0);
        Debug.Log("PlayerPrefs.GetInt(LanguageKey)"+currentLanguageIndex);
        // 유효한 인덱스(0,1,3)로 제한
        //if (currentLanguageIndex != 0 && currentLanguageIndex != 1 && currentLanguageIndex != 3)
        //{
        //    currentLanguageIndex = 0;
        //}
        ChangeLanguage(currentLanguageIndex);
        UpdateLanguageText();

    }

    public void OnLanguageButtonPressed(bool next)
    {
        // if (!SceneManager.GetActiveScene().name.Contains("TitleScene2"))
        //     return;

        Managers.Asset.PlaySound("Click", SoundType.UI);

        //1. 넣어져있는 언어 모두 변경가능할때 (0~n)
        // int localeCount = LocalizationSettings.AvailableLocales.Locales.Count;
        // if (next)
        // {
        //     currentLanguageIndex = (currentLanguageIndex + 1) % localeCount;
        // }
        // else
        // {
        //     currentLanguageIndex--;
        //     if (currentLanguageIndex < 0) currentLanguageIndex = localeCount - 1;
        // }

        //2. 한국어,영어만 변경가능할때 (0~1)
        //currentLanguageIndex = currentLanguageIndex == 0 ? 1 : 0;

        //3. 한국어, 영어, 중국어만 변경가능할때 (0,1,3)
        if (next)
        {
            if (currentLanguageIndex == 0) currentLanguageIndex = 1;
            else if (currentLanguageIndex == 1) currentLanguageIndex = 3;
            else if (currentLanguageIndex == 3) currentLanguageIndex = 0;
        }
        else
        {
            if (currentLanguageIndex == 0) currentLanguageIndex = 3;
            else if (currentLanguageIndex == 3) currentLanguageIndex = 1;
            else if (currentLanguageIndex == 1) currentLanguageIndex = 0;
        }


        ChangeLanguage(currentLanguageIndex);
        UpdateLanguageText();

        PlayerPrefs.SetInt(LanguageKey, currentLanguageIndex);
        PlayerPrefs.Save();
    }

    private void ChangeLanguage(int index)
    {
        // if (LocalizationSettings.AvailableLocales.Locales.Count > index)
        // {
        //     LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        // }

        // // 인덱스가 0 또는 1인 경우에만 언어 변경
        // if (index >= 0 && index <= 1 && LocalizationSettings.AvailableLocales.Locales.Count > index)
        // {
        //     LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        // }

        // 인덱스가 0, 1, 3인 경우에만 언어 변경
        if ((index == 0 || index == 1 || index == 3) && 
            LocalizationSettings.AvailableLocales.Locales.Count > index)
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        }
    }

    private void UpdateLanguageText()
    {
        var currentLocale = LocalizationSettings.SelectedLocale;
        //languageText.text = currentLocale.LocaleName;

        //languageText.GetComponent<LocalizeStringEvent>().StringReference.TableEntryReference = LocalizationKeys.GetTableKey("LANGUAGE");

        // talkSpeedText.GetComponent<LocalizeStringEvent>().StringReference.TableEntryReference
        //     = LocalizationKeys.SpeedKeys[typingSpeedIndex];
    }
    #endregion
    #endregion
}