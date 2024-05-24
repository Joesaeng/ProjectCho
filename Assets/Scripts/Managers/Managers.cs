using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers s_instance;
    static Managers Instance { get { Init(); return s_instance; } }

    #region Core
    PlayerManager _player = new PlayerManager();
    DataManager _data = new DataManager();
    InputManager _input = new InputManager();
    PoolManager _pool = new PoolManager();
    ResourceManager _resource = new ResourceManager();
    SoundManager _sound = new SoundManager();
    UIManager _UI = new UIManager();
    ComponentCacheManager _compCache = new ComponentCacheManager();

    // MonoBehaviour 상속 매니저
    SceneManagerEx _scene;

    public static DataManager Data { get { return Instance._data; } }
    public static InputManager Input { get { return Instance._input; } }
    public static PoolManager Pool { get { return Instance._pool; } }
    public static ResourceManager Resource { get { return Instance._resource; } }
    public static SoundManager Sound { get { return Instance._sound; } }
    public static UIManager UI { get { return Instance._UI; } }
    public static ComponentCacheManager CompCache { get {  return Instance._compCache; } }
    public static PlayerManager Player { get {  return Instance._player; } }

    // MonoBehaviour 상속 매니저
    public static SceneManagerEx Scene { get { return Instance._scene; } }
    #endregion

    void Start()
    {
        Init();
    }

    void Update()
    {
        _input.OnUpdate();
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
                s_instance._scene = go.GetOrAddComponent<SceneManagerEx>();
            }

            s_instance._data.Init();
            s_instance._pool.Init();
            s_instance._player.Init();
            s_instance._sound.Init();
        }
    }

    public static void Clear()
    {
        Sound.Clear();
        Input.Clear();
        Scene.Clear();
        UI.Clear();

        CompCache.Clear();
        Pool.Clear();
    }
}
