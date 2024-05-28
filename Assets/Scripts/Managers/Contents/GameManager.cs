using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    GameObject EnemysTarget { get; set; }
    public PlayerWall PlayerWall { get; set; }
    public List<Enemy> Enemies { get; set; } = new();

    List<Transform> MagicianPoints { get; set; } = new();
    List<Magician> Magicians { get; set; } = new();
    int MagicianCount { get; set; } = 0;

    GameObject SpawnArea;
    float LeftX { get; set; }
    float RightX { get; set; }
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

        Transform magicianPointParent = GameObject.Find("MagicainPoint").transform;
        for(int i = 0; i < magicianPointParent.childCount; ++i)
        {
            MagicianPoints.Add(magicianPointParent.GetChild(i));
        }
        
    }

    #region TEMP
    public void MouseEventListner(Define.MouseEvent mouseEvent)
    {
        if(mouseEvent == Define.MouseEvent.Click)
        {
            CreateMagician();
        }
    }

    public void KeyInput()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            CreateEnemy();
        }
    }

    void CreateMagician()
    {
        if(MagicianCount < MagicianPoints.Count)
        {
            GameObject obj = Managers.Resource.Instantiate("Magician", MagicianPoints[MagicianCount]);
            MagicianCount++;

            Magician magician = obj.GetOrAddComponent<Magician>();
            Magicians.Add(magician);
        }
    }

    void CreateEnemy()
    {
        float posX = Random.Range(LeftX, RightX);
        Vector3 spawnPos = new Vector3(posX, 0, PosZ);
        GameObject obj;

        int monsterId = 1;
        BaseEnemyData data = Managers.Data.BaseEnemyDataDict[monsterId];
        if(data.isRange)
        {
            obj = Managers.Resource.Instantiate("RangeEnemy",spawnPos);
            Managers.CompCache.GetOrAddComponentCache<RangeAttackEnemy>(obj);
        }
        else
        {
            obj = Managers.Resource.Instantiate("MeleeEnemy",spawnPos);
            Managers.CompCache.GetOrAddComponentCache<MeleeAttackEnemy>(obj);
        }

        Enemy enemy = obj.GetComponent<Enemy>();
        enemy.Init(data);
        enemy.SetDir(new Vector3(posX, 0, EnemysTarget.transform.position.z));

        Enemies.Add(enemy);
    }
    #endregion


    private void Update()
    {
        KeyInput();
        foreach (var enemy in Enemies)
        {
            enemy.OnUpdate();
        }
    }
}
