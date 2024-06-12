using Data;
using MagicianSpellUpgrade;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DefenseSceneManager : MonoBehaviour
{
    private static DefenseSceneManager instance;
    public static DefenseSceneManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (DefenseSceneManager)FindObjectOfType(typeof(DefenseSceneManager));
                if (instance == null)
                {
                    GameObject obj = new GameObject(typeof(DefenseSceneManager).Name, typeof(DefenseSceneManager));
                    instance = obj.GetComponent<DefenseSceneManager>();
                }
            }
            return instance;
        }
    }

    public PlayerWall PlayerWall { get; set; }
    public HashSet<Enemy> Enemies { get; set; }

    List<Transform> MagicianPoints { get; set; }
    List<Magician> Magicians { get; set; }
    int MagicianCount { get; set; }

    public SpellDataBase _SpellDataBase { get; set; }
    public EnemyDataBase _EnemyDataBase { get; set; }
    public HashSet<SpellUpgradeData> _SpellUpgradeDatas { get; set; }

    GameObject SpawnArea;
    public float LeftX { get; set; }
    public float RightX { get; set; }
    float PosZ { get; set; }
    float PosY { get; set; }

    private int PlayerLevel = 0;
    public float PlayerAttackPower = 10;

    #region StageData

    private int     CurStage = 0;
    private int     CurWave = 0;
    private int     EnemiesToSpawn;
    private int     EnemiesSpwaned = 0;
    private int     EnemiesDestroyed = 0;
    private float   SpawnInterval;

    private float   CurWaveTime = 0;
    private float   CurSpawnTime = 0;

    private readonly float OneWaveTime = 10f;

    #endregion

    #region UI
    UI_LevelUpPopup UI_LevelUpPopup;
    UI_DefenseScene UI_DefenseScene;

    // 이벤트
    public System.Action<List<LevelUpOptions>> OnSetLevelUpPopup;
    public System.Action<List<LevelUpOptions>> OnRerollLevelUpPopup;
    public System.Action<int,int> OnUpdatePlayerHp;
    #endregion

    public void Init()
    {
        CurStage = Managers.Game.SelectedStage;
        SpawnArea = GameObject.Find("EnemySpawnArea");
        PlayerWall = GameObject.Find("EnemyTarget").GetComponent<PlayerWall>();
        PlayerWall.InitHitable(new PlayerWallData() { id = 0, maxHp = 3000 });

        PosZ = Util.FindChild<Transform>(SpawnArea, "AreaLeftPos").position.z;
        PosY = Util.FindChild<Transform>(SpawnArea, "AreaLeftPos").position.y;
        LeftX = Util.FindChild<Transform>(SpawnArea, "AreaLeftPos").position.x;
        RightX = Util.FindChild<Transform>(SpawnArea, "AreaRightPos").position.x;

        Enemies = new();
        MagicianPoints = new();
        Magicians = new();
        MagicianCount = 0;
        _SpellDataBase = new();
        _EnemyDataBase = new();
        _SpellUpgradeDatas = new();

        Transform magicianPointParent = GameObject.Find("MagicianPoint").transform;
        for (int i = 0; i < magicianPointParent.childCount; ++i)
        {
            MagicianPoints.Add(magicianPointParent.GetChild(i));
        }

        _SpellDataBase.Init();
        _EnemyDataBase.Init();

        foreach (SpellUpgradeDatas datas in Managers.Data.UpgradeDataDict.Values)
        {
            foreach (SpellUpgradeData data in datas.spellUpgradeDatas)
            {
                _SpellUpgradeDatas.Add(data);
            }
        }

        #region UI 초기화
        UI_DefenseScene = Managers.UI.ShowSceneUI<UI_DefenseScene>();
        UI_DefenseScene.Init();

        UI_LevelUpPopup = Managers.UI.ShowPopupUI<UI_LevelUpPopup>();
        UI_LevelUpPopup.Init();
        UI_LevelUpPopup.OnClickedLevelUpOption += ClickedLevelUpOptionListner;
        UI_LevelUpPopup.OnClickedReroll += LevelUpOptionsReroll;
        UI_LevelUpPopup.gameObject.SetActive(false);

        PlayerWall.OnUpdatePlayerHp -= UpdatePlayerHpListner;
        PlayerWall.OnUpdatePlayerHp += UpdatePlayerHpListner;
        #endregion

        CreateMagician(Managers.Player.PlayerStatus.startingSpellId);
        StartStage(CurWave);
        Managers.Time.ChangeTimeScale();
    }

    void StartStage(int curWave)
    {
        StageData stageData = Managers.Data.StageDataDict[CurStage];
        WaveData waveData = stageData.stageDatas[curWave];
        _EnemyDataBase.SetEnemyStatusByStageData(curWave, stageData);
        EnemiesToSpawn = waveData.spawnEnemyCount;
        EnemiesSpwaned = 0;
        SpawnInterval = OneWaveTime / EnemiesToSpawn;
    }

    void NextStage()
    {
        CurWave++;

        StartStage(CurWave);
    }

    void LevelUp()
    {
        PlayerLevel++;

        OnSetLevelUpPopup.Invoke(CreateLevelUpOptions());
        Managers.Time.GamePause();
    }

    void LevelUpOptionsReroll()
    {
        OnRerollLevelUpPopup.Invoke(CreateLevelUpOptions());
    }

    public List<LevelUpOptions> CreateLevelUpOptions()
    {
        return LevelUpOptionsBuilder.CreateLevelUpOptions(Magicians);
    }

    void ClickedLevelUpOptionListner(LevelUpOptions option)
    {
        if (option.IsNewSpell)
            CreateMagician(option.SpellId);
        else
        {
            _SpellUpgradeDatas.Remove(option.SpellUpgradeData);
            SpellUpgradeFactory.CreateUpgrade(option.SpellUpgradeData);
            _SpellDataBase.UpgradeSkill(option.SpellId, MagicianSpellUpgrade.SpellUpgradeFactory.CreateUpgrade(option.SpellUpgradeData));
        }
        Managers.Time.GameResume();
    }

    void CreateMagician(int spellId)
    {
        if (MagicianCount < MagicianPoints.Count)
        {
            GameObject obj = Managers.Resource.Instantiate("Magician1", MagicianPoints[MagicianCount]);

            Magician magician = obj.GetOrAddComponent<Magician>();
            BaseSpellData data = Managers.Data.BaseSpellDataDict[spellId];
            GameObject aura = Managers.Resource.Instantiate($"Aura/Aura{data.elementType}",
                MagicianPoints[MagicianCount].position + new Vector3(0, 0.1f, 0));
            aura.transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 0));
            MagicianCount++;
            magician.Init(data);
            Magicians.Add(magician);
        }
    }

    void CreateEnemy()
    {
        float posX = Random.Range(LeftX, RightX);
        Vector3 spawnPos = new Vector3(posX, PosY, PosZ);
        GameObject obj;

        int randIndex = Random.Range(0,Managers.Data.StageDataDict[CurStage].stageDatas[CurWave].waveEnemyIds.Count);
        int monsterId = Managers.Data.StageDataDict[CurStage].stageDatas[CurWave].waveEnemyIds[randIndex];

        SetEnemyData setData = _EnemyDataBase.EnemyDataDict[monsterId];
        obj = Managers.Resource.Instantiate($"Enemys/{setData.prefabName}", spawnPos);
        if (setData.IsRange)
        {
            Managers.CompCache.GetOrAddComponentCache<RangeAttackEnemy>(obj, out RangeAttackEnemy enemy);
            enemy.Init(setData);
            Enemies.Add(enemy);
        }
        else
        {
            Managers.CompCache.GetOrAddComponentCache<MeleeAttackEnemy>(obj, out MeleeAttackEnemy enemy);
            enemy.Init(setData);
            Enemies.Add(enemy);
        }
    }

    public void KillEnemy(Enemy enemy)
    {
        Managers.Resource.Destroy(enemy.gameObject);
        Enemies.Remove(enemy);
        EnemiesDestroyed++;

        if (EnemiesDestroyed >= Managers.Data.StageDataDict[CurStage].stageDatas[PlayerLevel].spawnEnemyCount)
        {
            LevelUp();
            EnemiesDestroyed = 0;
        }
    }

    private void Update()
    {
        if (Managers.Time.IsPause)
            return;
        CurWaveTime += Time.deltaTime;
        CurSpawnTime += Time.deltaTime;

        if (CurSpawnTime >= SpawnInterval && EnemiesSpwaned < EnemiesToSpawn)
        {
            CreateEnemy();
            EnemiesSpwaned++;
            CurSpawnTime = 0;
        }

        if (CurWaveTime >= OneWaveTime)
        {
            NextStage();
            CurWaveTime = 0;
        }

        foreach (var magician in Magicians)
        {
            magician.OnUpdate();
        }
    }

    private void UpdatePlayerHpListner(int curHp)
    {
        OnUpdatePlayerHp.Invoke(curHp, Mathf.RoundToInt(PlayerWall.MaxHp));
    }

    public void Clear()
    {
        // 이벤트 구독 해제
        UI_LevelUpPopup.OnClickedLevelUpOption -= ClickedLevelUpOptionListner;
        UI_LevelUpPopup.OnClickedReroll -= LevelUpOptionsReroll;

        PlayerWall.OnUpdatePlayerHp -= UpdatePlayerHpListner;

        // 객체 제거
        Enemies = null;
        MagicianPoints = null;
        Magicians = null;
        MagicianCount = 0;
        _SpellDataBase = null;
        _EnemyDataBase = null;
        _SpellUpgradeDatas = null;

        // StageData 초기화
        CurStage = 0;
        CurWave = 0;
        EnemiesToSpawn = 0;
        EnemiesSpwaned = 0;
        EnemiesDestroyed = 0;
        SpawnInterval = 0;

        CurWaveTime = 0;
        CurSpawnTime = 0;

        // UI 제거
        UI_LevelUpPopup = null;
    }
}
