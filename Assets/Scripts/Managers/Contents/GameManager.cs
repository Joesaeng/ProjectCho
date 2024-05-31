using Data;
using System.Collections;
using System.Collections.Generic;
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

    GameObject SpawnArea;
    public float LeftX { get; set; }
    public float RightX { get; set; }
    float PosZ { get; set; }
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
    }

    #region TEMP
    public void MouseEventListner(Define.MouseEvent mouseEvent)
    {
        if (mouseEvent == Define.MouseEvent.Click)
        {
            CreateMagician();
        }
    }

    public void KeyInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CreateEnemy();
        }
    }

    void CreateMagician()
    {
        if (MagicianCount < MagicianPoints.Count)
        {
            GameObject obj = Managers.Resource.Instantiate("Magician", MagicianPoints[MagicianCount]);

            Magician magician = obj.GetOrAddComponent<Magician>();
            BaseSpellData data = Managers.Data.BaseSpellDataDict[MagicianCount];
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

    public void KillEnemy(Enemy enemy)
    {
        Managers.Resource.Destroy(enemy.gameObject);
        Enemies.Remove(enemy);
    }
    #endregion

    // TEMP
    float curTime = 0f;
    float enemySpawnTime = 0.5f;
    private void Update()
    {
        curTime += Time.deltaTime;
        if (curTime > enemySpawnTime)
        {
            CreateEnemy();
            curTime = 0f;
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
}
