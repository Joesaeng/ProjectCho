using Data;
using Define;
using Interfaces;
using JetBrains.Annotations;
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
                    GameObject obj = new(typeof(DefenseSceneManager).Name, typeof(DefenseSceneManager));
                    instance = obj.GetComponent<DefenseSceneManager>();
                }
            }
            return instance;
        }
    }

    public PlayerWall PlayerWall { get; set; }
    public HashSet<Enemy> Enemies { get; set; }
    Dictionary<ElementType, int> DefeatEnemiesByElementType { get; set; }

    List<Transform> SpellUseablePoints { get; set; }
    List<ISpellUseable> SpellUseables { get; set; }
    int SpellUseableCount { get; set; }

    public PlayerSpells PlayerSpells { get; set; }
    public EnemyDataBase EnemyDataBase { get; set; }
    public HashSet<SpellUpgradeData> SpellUpgradeDatas { get; set; }

    Dictionary<RewardType, int> StageRewardDict { get; set; }
    StageRewardData StageFirstClearReward { get; set; }

    GameObject SpawnArea;
    float LeftX { get; set; }
    float RightX { get; set; }
    float PosZ { get; set; }
    float PosY { get; set; }

    private int ClearWave = 0;

    #region StageData

    private int     CurStage = 0;
    private int     CurWave = 0;
    private int     EnemiesToSpawn;
    private int     EnemiesSpwaned = 0;
    private int     EnemiesDestroyed = 0;

    private float   SpawnInterval;

    private float   CurSpawnTime = 0;

    private bool    IsLastWave = false;
    private int     killCount = 0;
    #endregion

    #region UI
    UI_LevelUpPopup UI_LevelUpPopup;
    UI_DefenseScene UI_DefenseScene;
    UI_DefenseScenePause UI_DefenseScenePause;
    UI_GameOver UI_GameOver;

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
        PlayerWall.OnGameOver -= GameOver;
        PlayerWall.OnGameOver += GameOver;

        PosZ = Util.FindChild<Transform>(SpawnArea, "AreaLeftPos").position.z;
        PosY = Util.FindChild<Transform>(SpawnArea, "AreaLeftPos").position.y;
        LeftX = Util.FindChild<Transform>(SpawnArea, "AreaLeftPos").position.x;
        RightX = Util.FindChild<Transform>(SpawnArea, "AreaRightPos").position.x;

        Enemies = new();
        SpellUseablePoints = new();
        SpellUseables = new();
        SpellUseableCount = 0;
        PlayerSpells = Managers.Status.PlayerSpells;
        PlayerSpells.BuildSpellDict();
        EnemyDataBase = new();
        SpellUpgradeDatas = new();
        DefeatEnemiesByElementType = new();
        StageRewardDict = new();
        StageFirstClearReward = null;

        IsLastWave = false;

        for (int i = 0; i < System.Enum.GetValues(typeof(ElementType)).Length; ++i)
        {
            DefeatEnemiesByElementType[(ElementType)i] = 0;
        }

        Transform magicianPointParent = GameObject.Find("MagicianPoint").transform;
        for (int i = 0; i < magicianPointParent.childCount; ++i)
        {
            SpellUseablePoints.Add(magicianPointParent.GetChild(i));
        }

        EnemyDataBase.Init();

        foreach (SpellUpgradeDatas datas in Managers.Data.UpgradeDataDict.Values)
        {
            foreach (SpellUpgradeData data in datas.spellUpgradeDatas)
            {
                SpellUpgradeDatas.Add(data);
            }
        }

        #region UI 초기화
        UI_DefenseScene = Managers.UI.ShowSceneUI<UI_DefenseScene>();
        UI_DefenseScene.Init();
        UI_DefenseScene.OnClickedPause += PauseListner;
        UI_DefenseScene.OnClickedFast += FastListner;

        UI_LevelUpPopup = Managers.UI.ShowPopupUI<UI_LevelUpPopup>();
        UI_LevelUpPopup.Init();
        UI_LevelUpPopup.OnClickedLevelUpOption += ClickedLevelUpOptionListner;
        UI_LevelUpPopup.OnClickedReroll += LevelUpOptionsReroll;
        UI_LevelUpPopup.gameObject.SetActive(false);

        UI_DefenseScenePause = Managers.UI.ShowPopupUI<UI_DefenseScenePause>();
        UI_DefenseScenePause.Init();
        UI_DefenseScenePause.OnClickedResume += ResumeListner;
        UI_DefenseScenePause.OnClickedLobby += LobbyListner;
        UI_DefenseScenePause.gameObject.SetActive(false);

        UI_GameOver = Managers.UI.ShowPopupUI<UI_GameOver>();
        UI_GameOver.Init();
        UI_GameOver.OnClickedLobby += LobbyListner;
        UI_GameOver.gameObject.SetActive(false);

        PlayerWall.OnUpdatePlayerHp -= UpdatePlayerHpListner;
        PlayerWall.OnUpdatePlayerHp += UpdatePlayerHpListner;
        #endregion

        CreateMagician(Managers.Status.PlayerStatus.startingSpellId);
        StartWave(CurWave);
    }

    void StartWave(int curWave)
    {
        StageData stageData = Managers.Data.StageDataDict[CurStage];
        WaveData waveData = stageData.waveDatas[curWave];
        EnemyDataBase.SetEnemyStatusByStageData(curWave, stageData);
        EnemiesToSpawn = waveData.spawnEnemyCount;
        EnemiesSpwaned = 0;
        SpawnInterval = ConstantData.OneWaveTime / EnemiesToSpawn;

        UI_DefenseScene.SetWaveInfoText(curWave + 1, waveData.spawnEnemyCount);
    }

    void NextWave()
    {
        CurWave++;
        int lastWave = Managers.Data.StageDataDict[CurStage].waveDatas.Count - 1;

        if(CurWave == lastWave)
        {
            IsLastWave = true;
            UI_DefenseScene.ShowLastWaveText();
        }
        StartWave(CurWave);
    }

    #region LevelUp
    void WaveClear()
    {
        WaveRewardData waveRewardData = Managers.Data.StageDataDict[CurStage].waveDatas[ClearWave].waveRewardData;
        AddReward(waveRewardData.type, waveRewardData.value);

        int lastWave = Managers.Data.StageDataDict[CurStage].waveDatas.Count - 1;
        if (ClearWave == lastWave)
            return;

        ClearWave++;
        OnSetLevelUpPopup.Invoke(CreateLevelUpOptions());
        Managers.Time.GamePause();
    }

    void AddReward(RewardType rewardType,int value)
    {
        if (StageRewardDict.ContainsKey(rewardType))
            StageRewardDict[rewardType] += value;
        else
            StageRewardDict[rewardType] = value;
    }

    void LevelUpOptionsReroll()
    {
        OnRerollLevelUpPopup.Invoke(CreateLevelUpOptions());
    }

    public List<LevelUpOptions> CreateLevelUpOptions()
    {
        return LevelUpOptionsBuilder.CreateLevelUpOptions(SpellUseables);
    }

    void ClickedLevelUpOptionListner(LevelUpOptions option)
    {
        if (option.IsNewSpell)
        {
            CreateCharge(option.SpellId);
        }
        else
        {
            SpellUpgradeDatas.Remove(option.SpellUpgradeData);
            SpellUpgradeFactory.CreateUpgrade(option.SpellUpgradeData);
            PlayerSpells.UpgradeSkill(option.SpellId, MagicianSpellUpgrade.SpellUpgradeFactory.CreateUpgrade(option.SpellUpgradeData));
            UI_DefenseScene.LevelUpUsingSpell(option.SpellId);
        }
        Managers.Time.GameResume();
    }
    #endregion

    #region SpellUseable
    void CreateMagician(int spellId)
    {
        if (SpellUseableCount < SpellUseablePoints.Count)
        {
            GameObject obj = Managers.Resource.Instantiate("Magician1", SpellUseablePoints[SpellUseableCount]);

            Magician magician = obj.GetOrAddComponent<Magician>();
            CreateSpellUseable(spellId, magician);
        }
    }

    void CreateCharge(int spellId)
    {
        if (SpellUseableCount < SpellUseablePoints.Count)
        {
            GameObject obj = Managers.Resource.Instantiate("SpellCharge", SpellUseablePoints[SpellUseableCount]);
            obj.transform.localPosition += Vector3.up;

            SpellCharge spellCharge = obj.GetOrAddComponent<SpellCharge>();
            CreateSpellUseable(spellId, spellCharge);
        }
    }

    void CreateSpellUseable(int spellId, ISpellUseable spellUseable)
    {
        BaseSpellData data = Managers.Data.BaseSpellDataDict[spellId];
        GameObject aura = Managers.Resource.Instantiate($"Aura/Aura{data.elementType}",
                SpellUseablePoints[SpellUseableCount].position + new Vector3(0, 0.1f, 0));
        aura.transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 0));
        spellUseable.Init(data);
        UI_DefenseScene.SetUsingSpell(spellId, SpellUseableCount);
        SpellUseableCount++;
        SpellUseables.Add(spellUseable);
    }
    #endregion

    #region Enemy
    void CreateEnemy()
    {
        float posX = Random.Range(LeftX, RightX);
        Vector3 spawnPos = new(posX, PosY, PosZ);
        GameObject obj;

        int randIndex = Random.Range(0,Managers.Data.StageDataDict[CurStage].waveDatas[CurWave].waveEnemyIds.Count);
        int monsterId = Managers.Data.StageDataDict[CurStage].waveDatas[CurWave].waveEnemyIds[randIndex];

        SetEnemyData setData = EnemyDataBase.EnemyDataDict[monsterId];
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

    public void DefeatEnemy(Enemy enemy)
    {
        Managers.Resource.Destroy(enemy.gameObject);
        Enemies.Remove(enemy);

        DefeatEnemiesByElementType[enemy.ElementType]++;
        killCount++;
        UI_DefenseScene.SetKillCount(killCount);

        EnemiesDestroyed++;
        int requireLevelUp = Managers.Data.StageDataDict[CurStage].waveDatas[ClearWave].spawnEnemyCount;
        if (EnemiesDestroyed >= requireLevelUp)
        {
            WaveClear();
            EnemiesDestroyed = 0;
        }
        if(IsLastWave && Enemies.Count <= 0)
        {
            if(Managers.PlayerData.StageClearList.Contains(CurStage) == false)
            {
                Managers.PlayerData.AddClearStage(CurStage);
                StageFirstClearReward = Managers.Data.StageDataDict[CurStage].firstClearRewardData;
            }
            GameOver(GameoverType.Clear);
        }
        float expGaugeFill = (float)EnemiesDestroyed / requireLevelUp;
        UI_DefenseScene.SetExpGauge(expGaugeFill);
    }
    #endregion

    #region UI

    private void FastListner()
    {
        Managers.Time.ChangeTimeScale();
    }

    private void PauseListner()
    {
        Managers.Time.GamePause();
        UI_DefenseScenePause.gameObject.SetActive(true);
        UI_DefenseScenePause.SetDefeatEnemies(DefeatEnemiesByElementType);
    }

    private void ResumeListner()
    {
        Managers.Time.GameResume();
        UI_DefenseScenePause.gameObject.SetActive(false);
    }

    private void LobbyListner()
    {
        Managers.Scene.LoadSceneWithLoadingScene(Define.Scene.Lobby);
    }

    private void UpdatePlayerHpListner(int curHp)
    {
        OnUpdatePlayerHp.Invoke(curHp, Mathf.RoundToInt(PlayerWall.MaxHp));
    }
    #endregion

    private void Update()
    {
        if (Managers.Time.IsPause)
            return;
        CurSpawnTime += Time.deltaTime;

        if (CurSpawnTime >= SpawnInterval && EnemiesSpwaned < EnemiesToSpawn)
        {
            CreateEnemy();
            EnemiesSpwaned++;
            CurSpawnTime = 0;
        }

        if (IsLastWave == false && EnemiesSpwaned >= EnemiesToSpawn)
        {
            NextWave();
        }

        foreach (ISpellUseable spellUseable in SpellUseables)
        {
            spellUseable.OnUpdate();
        }
    }

    void ApplyAchievementDefeatEnemies()
    {
        foreach (var elementTypeEnemyPair in DefeatEnemiesByElementType)
        {
            Managers.Achieve.SetAchievementValueByTargetType(AchievementTargetType.DefeatEnemies,
                elementTypeEnemyPair.Value, elementType: elementTypeEnemyPair.Key);
        }
    }

    void GameOver(GameoverType type)
    {
        Managers.Time.GamePause();
        UI_GameOver.ShowGameOverUI();
        UI_GameOver.SetDefeatEnemies(DefeatEnemiesByElementType);
        UI_GameOver.SetGameoverUI(type,StageRewardDict,StageFirstClearReward);

        ApplyAchievementDefeatEnemies();
        foreach(var reward in StageRewardDict)
        {
            GainReward(reward.Key,reward.Value);
        }
        if(StageFirstClearReward != null)
        {
            GainReward(StageFirstClearReward.type, StageFirstClearReward.value);
        }
    }

    void GainReward(RewardType type, int value)
    {
        switch (type)
        {
            case RewardType.RewardDia:
                Managers.PlayerData.IncreaseDia(value);
                break;
            case RewardType.RewardCoins:
                Managers.PlayerData.IncreaseCoins(value);
                break;
        }
    }

    public void Clear()
    {
        PlayerSpells.ClearSpellDict();
    }
}