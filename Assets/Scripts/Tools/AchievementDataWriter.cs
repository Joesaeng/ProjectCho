using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementDataWriter : MonoBehaviour
{
    string jsonpath = "Resources/Data/AchievementData.json";

    public string achievementName;
    public AchievementType type;
    public AchievementTargetData target;
    public List<AchievementRewardData> rewards;

    public void WriteData()
    {
        AchievementData newData = new AchievementData()
        {
            achievementName = achievementName,
            type = type,
            target = target,
            rewards = rewards
        };
        JsonDataWriter.WriteData(jsonpath, newData);
    }
}
