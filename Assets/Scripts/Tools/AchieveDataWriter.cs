using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchieveDataWriter : MonoBehaviour
{
    string jsonpath = "Resources/Data/AchieveData.json";

    public string achieveName;
    public AchieveType type;
    public AchieveTargetData target;
    public List<AchieveRewardData> rewards;

    public void WriteData()
    {
        AchieveData newData = new AchieveData()
        {
            achieveName = achieveName,
            type = type,
            target = target,
            rewards = rewards
        };
        JsonDataWriter.WriteData(jsonpath, newData);
    }
}
