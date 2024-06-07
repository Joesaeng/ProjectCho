using Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    GameObject EnemysTarget { get; set; }
    public PlayerWall PlayerWall { get; set; }
    public HashSet<Enemy> Enemies { get; set; } = new();

    List<Transform> MagicianPoints { get; set; } = new();
    List<Magician> Magicians { get; set; } = new();
    int MagicianCount { get; set; } = 0;

    public SpellDataBase _SpellDataBase { get; } = new();
    public EnemyDataBase _EnemyDataBase { get; } = new();
    public HashSet<SpellUpgradeData> _SpellUpgradeDatas { get; set; } = new();
    LevelUpOptionsBuilder _LevelUpOptionsBuilder { get; } = new();

    GameObject SpawnArea;
    public float LeftX { get; set; }
    public float RightX { get; set; }
    float PosZ { get; set; }
    float PosY { get; set; }

    private int PlayerLevel = 0;
    public float PlayerAttackPower = 10;

    #region StageData

    private int CurLevel = 0;
    private int CurStage = 0;
    private int EnemiesToSpawn;
    private int EnemiesSpwaned = 0;
    private int EnemiesDestroyed = 0;
    private float SpawnInterval;

    private float CurStageTime = 0;
    private float CurSpawnTime = 0;
    private readonly float OneStageTime = 10f;

    #endregion

    #region UI에게 보낼 이벤트
    public System.Action<int,int> OnUpdatePlayerHp;
    #endregion

    private void Start()
    {
        SpawnArea = GameObject.Find("EnemySpawnArea");
        EnemysTarget = GameObject.Find("EnemyTarget");
        PlayerWall = EnemysTarget.GetComponent<PlayerWall>();
        PlayerWall.InitHitable(new PlayerWallData() { id = 0, maxHp = 3000 });

        PosZ = Util.FindChild<Transform>(SpawnArea, "AreaLeftPos").position.z;
        LeftX = Util.FindChild<Transform>(SpawnArea, "AreaLeftPos").position.x;
        RightX = Util.FindChild<Transform>(SpawnArea, "AreaRightPos").position.x;
        PosY = Util.FindChild<Transform>(SpawnArea, "AreaLeftPos").position.y;

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

        #region UI용 이벤트 바인드
        PlayerWall.OnUpdatePlayerHp -= UpdatePlayerHpListner;
        PlayerWall.OnUpdatePlayerHp += UpdatePlayerHpListner;
        #endregion

        CreateMagician(0);
        StartStage(CurStage);
    }

    void StartStage(int stage)
    {
        LevelData levelData = Managers.Data.LevelDataDict[0];
        StageData stageData = levelData.stageDatas[stage];
        _EnemyDataBase.SetEnemyStatusByStageData(stage, levelData);
        EnemiesToSpawn = stageData.spawnEnemyCount;
        EnemiesSpwaned = 0;
        SpawnInterval = OneStageTime / EnemiesToSpawn;
    }

    void NextStage()
    {
        CurStage++;

        StartStage(CurStage);
    }


    public List<LevelUpOptions> SetLevelUpOptions { get; set; }
    void LevelUp()
    {
        PlayerLevel++;
        CreateLevelUpOptions();

        Managers.UI.ShowPopupUI<UI_LevelUpPopup>().OnClickedLevelUpOption += ClickedLevelUpOptionListner;
        Managers.Time.GamePause();
    }

    public void CreateLevelUpOptions()
    {
        SetLevelUpOptions = _LevelUpOptionsBuilder.CreateLevelUpOptions(Magicians);
    }

    void ClickedLevelUpOptionListner(LevelUpOptions option)
    {
        if (option.IsNewSpell)
            CreateMagician(option.SpellId);
        else
        {
            _SpellUpgradeDatas.Remove(option.SpellUpgradeData);
            MagicianSpellUpgrade.SpellUpgradeFactory.CreateUpgrade(option.SpellUpgradeData);
            _SpellDataBase.UpgradeSkill(option.SpellId, MagicianSpellUpgrade.SpellUpgradeFactory.CreateUpgrade(option.SpellUpgradeData));
        }
        Managers.Time.GameResume();
    }

    void CreateMagician(int spellId)
    {
        if (MagicianCount < MagicianPoints.Count)
        {
            GameObject obj = Managers.Resource.Instantiate("Magician", MagicianPoints[MagicianCount]);

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

        int randIndex = Random.Range(0,Managers.Data.LevelDataDict[CurLevel].stageDatas[CurStage].spawnEnemyIds.Count);
        int monsterId = Managers.Data.LevelDataDict[CurLevel].stageDatas[CurStage].spawnEnemyIds[randIndex];

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

        if(EnemiesDestroyed >= Managers.Data.LevelDataDict[CurLevel].stageDatas[PlayerLevel].spawnEnemyCount)
        {
            LevelUp();
            EnemiesDestroyed = 0;
        }    
    }

    private void Update()
    {
        if (Managers.Time.IsPause)
            return;
        CurStageTime += Time.deltaTime;
        CurSpawnTime += Time.deltaTime;

        if (CurSpawnTime >= SpawnInterval && EnemiesSpwaned < EnemiesToSpawn)
        {
            CreateEnemy();
            EnemiesSpwaned++;
            CurSpawnTime = 0;
        }

        if(CurStageTime >= OneStageTime)
        {
            NextStage();
            CurStageTime = 0;
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
}
