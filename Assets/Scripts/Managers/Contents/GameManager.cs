using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    GameObject EnemysTarget;
    GameObject SpawnArea;
    private void Start()
    {
        Managers.Input.MouseAction += MouseEventListner;
        SpawnArea = GameObject.Find("EnemySpawnArea");
        EnemysTarget = GameObject.Find("Wall");
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
        float posZ = Util.FindChild<Transform>(SpawnArea, "AreaLeftPos").position.z;
        float leftX = Util.FindChild<Transform>(SpawnArea, "AreaLeftPos").position.x;
        float rightX = Util.FindChild<Transform>(SpawnArea, "AreaRightPos").position.x;

        float posX = Random.Range(leftX, rightX);
        Vector3 spawnPos = new Vector3(posX, 1, posZ);
        GameObject obj = Managers.Resource.Instantiate("Enemy",spawnPos);
        Enemy enemy;
        Managers.CompCache.GetOrAddComponentCache(obj, out enemy);

        enemy.Init(Managers.Data.BaseEnemyDataDict[0]);
        enemy.SetDestination(new Vector3(posX, 1, EnemysTarget.transform.position.z));

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
