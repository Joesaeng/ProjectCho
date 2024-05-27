using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    GameObject EnemysTarget;
    PlayerWall PlayerWall;

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
    }
    // TEMP
    public void MouseEventListner(Define.MouseEvent mouseEvent)
    {
        if(mouseEvent == Define.MouseEvent.Click)
        {
            CreateEnemy();
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

        _creatures.Add(enemy);
    }
    //

    List<Creature> _creatures = new();

    private void Update()
    {
        foreach(var creature in _creatures)
        {
            creature.OnUpdate();
        }
    }
}
