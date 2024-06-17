using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers s_instance;
    static Managers Instance { get { Init(); return s_instance; } }

    GameManager _game = new();
    PlayerStatusManager _playerStatus = new();
    TimerManager _timer;
    TimeManager _time = new();
    ItemManager _item = new();
    AchievementManager _achieve = new();

    public static GameManager Game { get { return Instance._game; } }
    public static PlayerStatusManager Player { get { return Instance._playerStatus; } }
    public static TimerManager Timer { get { return Instance._timer; } }
    public static TimeManager Time { get { return Instance._time; } }
    public static ItemManager Item { get { return Instance._item; } }
    public static AchievementManager Achieve { get { return Instance._achieve; } }
    #region Core
    PlayerDataManager _playerData = new();
    DataManager _data = new();
    InputManager _input = new();
    PoolManager _pool = new();
    ResourceManager _resource = new();
    SoundManager _sound = new();
    UIManager _UI = new();
    ComponentCacheManager _compCache = new();

    SceneManagerEx _scene;

    public static DataManager Data { get { return Instance._data; } }
    public static InputManager Input { get { return Instance._input; } }
    public static PoolManager Pool { get { return Instance._pool; } }
    public static ResourceManager Resource { get { return Instance._resource; } }
    public static SoundManager Sound { get { return Instance._sound; } }
    public static UIManager UI { get { return Instance._UI; } }
    public static ComponentCacheManager CompCache { get {  return Instance._compCache; } }
    public static PlayerDataManager PlayerData { get {  return Instance._playerData; } }


    public static SceneManagerEx Scene { get { return Instance._scene; } }
    #endregion

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
                s_instance._timer = go.GetOrAddComponent<TimerManager>();
                s_instance._scene = go.GetOrAddComponent<SceneManagerEx>();
            }

            s_instance._time.Init();
            s_instance._data.Init();
            s_instance._item.Init();
            s_instance._playerData.Init();
            s_instance._achieve.Init();
            s_instance._playerStatus.Init();
            // s_instance._sound.Init();
        }
    }

    public static void Clear()
    {
        // Sound.Clear();
        Input.Clear();
        Scene.Clear();
        UI.Clear();
        Pool.Clear();
        
    }
}
