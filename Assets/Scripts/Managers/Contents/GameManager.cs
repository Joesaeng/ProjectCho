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

    #region UI에게 보낼 이벤트
    public System.Action<int,int> OnUpdatePlayerHp;
    #endregion

    private void Start()
    {
        Managers.Input.MouseAction += MouseEventListner;
        SpawnArea = GameObject.Find("EnemySpawnArea");
        EnemysTarget = GameObject.Find("EnemyTarget");
        PlayerWall = EnemysTarget.GetComponent<PlayerWall>();
        PlayerWall.InitHitable(new PlayerWallData() { id = 0, maxHp = 3000 });

        PosZ = Util.FindChild<Transform>(SpawnArea, "AreaLeftPos").position.z;
        LeftX = Util.FindChild<Transform>(SpawnArea, "AreaLeftPos").position.x;
        RightX = Util.FindChild<Transform>(SpawnArea, "AreaRightPos").position.x;

        Transform magicianPointParent = GameObject.Find("MagicianPoint").transform;
        for (int i = 0; i < magicianPointParent.childCount; ++i)
        {
            MagicianPoints.Add(magicianPointParent.GetChild(i));
        }

        _SpellDataBase.Init();
        _EnemyDataBase.Init();

        foreach(SpellUpgradeDatas datas in Managers.Data.UpgradeDataDict.Values)
        {
            foreach(SpellUpgradeData data in datas.spellUpgradeDatas)
            {
                _SpellUpgradeDatas.Add(data);
            }
        }

        #region UI용 이벤트 바인드
        PlayerWall.OnUpdatePlayerHp -= UpdatePlayerHpListner;
        PlayerWall.OnUpdatePlayerHp += UpdatePlayerHpListner;
        #endregion

        LevelUp();
    }

    #region TEMP
    public void MouseEventListner(Define.MouseEvent mouseEvent)
    {
        if (mouseEvent == Define.MouseEvent.Click)
        {
            // CreateMagician();
            // LevelUp();
        }
    }

    public void KeyInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CreateEnemy();
        }
    }

    public List<LevelUpOptions> SetLevelUpOptions { get; set; }
    void LevelUp()
    {
        SetLevelUpOptions = _LevelUpOptionsBuilder.CreateLevelUpOptions(Magicians);

        Managers.UI.ShowPopupUI<UI_LevelUpPopup>().OnClickedLevelUpOption += ClickedLevelUpOptionListner;
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
        Vector3 spawnPos = new Vector3(posX, 0, PosZ);
        GameObject obj;

        int monsterId = Random.Range(0,2);
        // BaseEnemyData data = Managers.Data.BaseEnemyDataDict[monsterId];
        SetEnemyData setData = _EnemyDataBase.EnemyDataDict[monsterId];
        if (setData.IsRange)
        {
            obj = Managers.Resource.Instantiate("Enemys/RangeEnemy0", spawnPos);
            Managers.CompCache.GetOrAddComponentCache<RangeAttackEnemy>(obj);
        }
        else
        {
            obj = Managers.Resource.Instantiate("Enemys/MeleeEnemy0", spawnPos);
            Managers.CompCache.GetOrAddComponentCache<MeleeAttackEnemy>(obj);
        }

        Enemy enemy = obj.GetComponent<Enemy>();
        enemy.Init(setData);
        enemy.SetDir(new Vector3(0, 0, -1));

        Enemies.Add(enemy);
    }

    int killCount = 0;
    int levelUpKillCount = 0;
    public void KillEnemy(Enemy enemy)
    {
        Managers.Resource.Destroy(enemy.gameObject);
        Enemies.Remove(enemy);
        killCount++;
        if (killCount > enemyCountByStage[levelUpKillCount])
        {
            LevelUp();
            levelUpKillCount++;
            killCount = 0;
        }
    }
    #endregion

    // TEMP
    static int[] enemyCountByStage = new int[]{5,10,20,40,60,80,100,100,100,100};
    static int stage = 0;
    float curSpawnTime = 0f;
    float stageTime = 0f;
    float enemySpawnTime = 20 / enemyCountByStage[stage];
    private void Update()
    {
        curSpawnTime += Time.deltaTime;
        stageTime += Time.deltaTime;
        if (curSpawnTime > enemySpawnTime)
        {
            CreateEnemy();
            curSpawnTime = 0f;
        }
        if(stageTime > 20f)
        {
            stage++;
            stageTime = 0f;
        }
        KeyInput();
        foreach (var enemy in Enemies)
        {
            enemy.OnUpdate();
        }
        foreach (var magician in Magicians)
        {
            magician.OnUpdate();
        }
    }

    private void UpdatePlayerHpListner(int curHp)
    {
        OnUpdatePlayerHp.Invoke(curHp,Mathf.RoundToInt(PlayerWall.MaxHp));
    }
}
