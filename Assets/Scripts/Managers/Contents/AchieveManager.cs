using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Achieve
{
    public string achieveName;
    public bool isClear;
    public AchieveType type;
    public AchieveTarget target;
    public List<AchieveReward> rewards;

    public Achieve(AchieveData achieveData)
    {
        achieveName = achieveData.achieveName;
        type = achieveData.type;
        target = new AchieveTarget(achieveData.target);
        rewards = achieveData.rewards.Select(rewardData => new AchieveReward(rewardData)).ToList();
    }

    public AchieveData ToData()
    {
        return new AchieveData
        {
            achieveName = this.achieveName,
            isClear = this.isClear,
            type = this.type,
            target = this.target.ToData(),
            rewards = this.rewards.Select(reward => reward.ToData()).ToList()
        };
    }
}
public class AchieveTarget
{
    public AchieveTargetType type;
    public ElementType elementType;
    public EquipmentType summonType;
    public int targetValue;
    public int progressValue;

    public AchieveTarget(AchieveTargetData achieveTargetData)
    {
        type = achieveTargetData.type;
        elementType = achieveTargetData.elementType;
        summonType = achieveTargetData.summonType;
        targetValue = achieveTargetData.targetValue;
    }

    public AchieveTargetData ToData()
    {
        return new AchieveTargetData
        {
            type = this.type,
            elementType = this.elementType,
            summonType = this.summonType,
            targetValue = this.targetValue,
            progressValue = this.progressValue
        };
    }
}

public class AchieveReward
{
    public AchieveRewardType type;
    public StatusType statusType;
    public int integerParam;
    public float floatParam;

    public AchieveReward(AchieveRewardData achieveRewardData)
    {
        type = achieveRewardData.type;
        statusType = achieveRewardData.statusType;
        integerParam = achieveRewardData.integerParam;
        floatParam = achieveRewardData.floatParam;
    }

    public AchieveRewardData ToData()
    {
        return new AchieveRewardData
        {
            type = this.type,
            statusType = this.statusType,
            integerParam = this.integerParam,
            floatParam = this.floatParam,
        };
    }
}
public class AchieveManager
{
    private List<Achieve> _achieves;
    public List<Achieve> Achieves { get => _achieves; set => _achieves = value; }

    public void Init()
    {
        Managers.PlayerData.Data.achieveDatas.Select(data => new Achieve(data)).ToList();
    }

    public List<AchieveData> ToData()
    {
        return _achieves.Select(achieve => achieve.ToData()).ToList();
    }
}
