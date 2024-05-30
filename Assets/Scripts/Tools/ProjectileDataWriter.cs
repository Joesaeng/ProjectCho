using Data;
using Define;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ProjectileDataWriter : MonoBehaviour
{
    string jsonpath = "Resources/Data/ProjectileData.json";

    public float baseMoveSpeed;
    [Header("ProjectTilePrefab의 경로는 Resources/Prefabs/Projectiles 이어야 합니다")]
    public GameObject projectilePrefab;
    [Header("ExplosionPrefab 경로는 Resources/Prefabs/Explosion 이어야 합니다")]
    public GameObject explosionPrefab;
    string projectileName;
    string explosionName;

    public void WriteData()
    {
        projectileName = projectilePrefab.name;
        explosionName = explosionPrefab.name;
        ProjectileData newData = new ProjectileData()
        {
            baseMoveSpeed = baseMoveSpeed,
            projectileName = projectileName,
            explosionName = explosionName,
        };
        JsonDataWriter.WriteData(jsonpath, newData);
    }
}