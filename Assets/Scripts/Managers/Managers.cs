using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers s_instance;
    static Managers Instance { get { Init(); return s_instance; } }

    GameManager _game;
    PlayerStatusManager _playerStatus = new();
    TimerManager _timer;
    TimeManager _time = new();

    public static GameManager Game { get { return Instance._game; } }
    public static PlayerStatusManager Player { get { return Instance._playerStatus; } }
    public static TimerManager Timer { get { return Instance._timer; } }
    public static TimeManager Time { get { return Instance._time; } }
    #region Core
    PlayerDataManager _playerData = new PlayerDataManager();
    DataManager _data = new DataManager();
    InputManager _input = new InputManager();
    PoolManager _pool = new PoolManager();
    ResourceManager _resource = new ResourceManager();
    SoundManager _sound = new SoundManager();
    UIManager _UI = new UIManager();
    ComponentCacheManager _compCache = new ComponentCacheManager();

    // MonoBehaviour ��� �Ŵ���
    SceneManagerEx _scene;

    public static DataManager Data { get { return Instance._data; } }
    public static InputManager Input { get { return Instance._input; } }
    public static PoolManager Pool { get { return Instance._pool; } }
    public static ResourceManager Resource { get { return Instance._resource; } }
    public static SoundManager Sound { get { return Instance._sound; } }
    public static UIManager UI { get { return Instance._UI; } }
    public static ComponentCacheManager CompCache { get {  return Instance._compCache; } }
    public static PlayerDataManager PlayerData { get {  return Instance._playerData; } }

    // MonoBehaviour ��� �Ŵ���
    public static SceneManagerEx Scene { get { return Instance._scene; } }
    #endregion

    void Start()
    {
        Init();
    }

    void Update()
    {
        _input.OnUpdate();
        _time.OnUpdate();
    }

    public static void Init()
    {
        if(s_instance == null)
        {
            GameObject go = GameObject.Find("@Managers");
            if(go == null)
            {
                go = new GameObject { name = "@Managers" };
                go.AddComponent<Managers>();
            }

            DontDestroyOnLoad(go);
            s_instance = go.GetComponent<Managers>();

            // 초기화가 필요한 매니저들 초기화 작업

            // MonoBehaviour를 상속받은 매니저들
            {
                s_instance._game = go.GetOrAddComponent<GameManager>();
                s_instance._timer = go.GetOrAddComponent<TimerManager>();
                s_instance._scene = go.GetOrAddComponent<SceneManagerEx>();
            }

            s_instance._time.Init();
            s_instance._data.Init();
            // s_instance._playerData.Init();
            // s_instance._sound.Init();
        }
    }

    public static void Clear()
    {
        Sound.Clear();
        Input.Clear();
        Scene.Clear();
        UI.Clear();
        Game.Clear();

        CompCache.Clear();
        Pool.Clear();
    }
}
