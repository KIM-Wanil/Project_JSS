using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Managers : MonoBehaviour
{
    private static Managers _instance;
    public static Managers Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<Managers>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("@Managers");
                    _instance = go.AddComponent<Managers>();

                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    [SerializeField] private DBManager _db;
    public static DBManager DB => Instance._db;

    [SerializeField] private AssetManager _asset;
    public static AssetManager Asset => Instance._asset;

    //[SerializeField] private SettingsManager _settings;
    //public static SettingsManager Settings => Instance._settings;

    [SerializeField] private UIManager _ui;
    public static UIManager UI => Instance._ui;

    [SerializeField] private GameManager _game;
    public static GameManager Game => Instance._game;

    [SerializeField] private SaveManager _save;
    public static SaveManager Save => Instance._save;

    [SerializeField] private GridManager _grid;
    public static GridManager Grid => Instance._grid;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        InitManagers();
    }

    private void InitManagers()
    {
        _db.Init();
        _asset.Init();
        //_settings.Init();
        _ui.Init();
        _game.Init();
        _save.Init();
        _grid.Init();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

        //_ui.UpdateLetterbox();
        //_settings.Init();
        Debug.Log($"OnSceneLoaded {scene}");
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}