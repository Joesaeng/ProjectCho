using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.SocialPlatforms.Impl;

public class Achievement
{
    public int id;
    public string achievementName;
    public bool isCompleted;
    public AchievementType type;
    public AchievementTarget target;
    public List<AchievementReward> rewards;

    public Achievement(AchievementData data)
    {
        id = data.id;
        achievementName = data.achievementName;
        isCompleted = data.isCompleted;
        type = data.type;
        target = new AchievementTarget(data.target);
        rewards = data.rewards.Select(rewardData => new AchievementReward(rewardData)).ToList();
    }

    public AchievementData ToData()
    {
        return new AchievementData
        {
            id = id,
            achievementName = this.achievementName,
            isCompleted = this.isCompleted,
            type = this.type,
            target = this.target.ToData(),
            rewards = this.rewards.Select(reward => reward.ToData()).ToList()
        };
    }
}
public class AchievementTarget
{
    public AchievementTargetType type;
    public ElementType elementType;
    public SummonType summonType;
    public int targetValue;
    public int progressValue;

    public AchievementTarget(AchievementTargetData data)
    {
        type = data.type;
        elementType = data.elementType;
        summonType = data.summonType;
        targetValue = data.targetValue;
        progressValue = data.progressValue;
    }

    public AchievementTargetData ToData()
    {
        return new AchievementTargetData
        {
            type = this.type,
            elementType = this.elementType,
            summonType = this.summonType,
            targetValue = this.targetValue,
            progressValue = this.progressValue
        };
    }
}

public class AchievementReward
{
    public RewardType type;
    public StatusType statusType;
    public int integerParam;
    public float floatParam;

    public AchievementReward(AchievementRewardData data)
    {
        type = data.type;
        statusType = data.statusType;
        integerParam = data.integerParam;
        floatParam = data.floatParam;
    }

    public AchievementRewardData ToData()
    {
        return new AchievementRewardData
        {
            type = this.type,
            statusType = this.statusType,
            integerParam = this.integerParam,
            floatParam = this.floatParam,
        };
    }
}
public class AchievementManager
{
    public Dictionary<AchievementType,List<Achievement>> _completedAchievements;
    public Dictionary<AchievementType,List<Achievement>> _pendingAchievements;

    public Action<AchievementType> OnAchievementCompletable;
    public Action<AchievementType> OnAchievementComplete;

    public void Init()
    {
        _completedAchievements = new();
        _pendingAchievements = new();
        foreach (var data in Managers.Data.AchievementDataDict.Values)
        {
            if (!Managers.PlayerData.AchievementDatas.Any(d => d.id == data.id))
            {
                Managers.PlayerData.AchievementDatas.Add(data);
            }
        }

        List<Achievement> achievements = Managers.PlayerData.AchievementDatas.Select(data => new Achievement(data)).ToList();

        foreach (var achievement in achievements)
        {
            if (achievement.isCompleted)
            {
                if (_completedAchievements.ContainsKey(achievement.type))
                    _completedAchievements[achievement.type].Add(achievement);
                else
                    _completedAchievements[achievement.type] = new List<Achievement>() { achievement };

                // 이미 클리어 한 업적 중 status를 상승시키는 업적이 있을 때 적용
                List<AchievementReward> statusRewards = achievement.rewards.Where(data => data.type == RewardType.RewardStatus).ToList();
                foreach(AchievementReward statusReward in statusRewards)
                {
                    RewardPlayerForAchievement(statusReward);
                }
            }
            else
            {
                if (_pendingAchievements.ContainsKey(achievement.type))
                    _pendingAchievements[achievement.type].Add(achievement);
                else
                    _pendingAchievements[achievement.type] = new List<Achievement>() { achievement };
            }
        }
    }

    public void CompleteAchievement(Achievement achievement)
    {
        if (_pendingAchievements.TryGetValue(achievement.type, out var achievements))
        {
            foreach (var ac in achievements)
            {
                if (ac.id == achievement.id)
                {
                    if (ac.target.progressValue < ac.target.targetValue)
                        return;
                    if (ac.type != AchievementType.Repeat)
                    {
                        ac.isCompleted = true;

                        if (_completedAchievements.TryGetValue(ac.type, out var completedAchievements))
                        {
                            completedAchievements.Add(ac);
                        }
                        else
                        {
                            _completedAchievements[ac.type] = new List<Achievement> { ac };
                        }
                        achievements.Remove(ac);
                    }
                    else
                        ac.target.progressValue -= ac.target.targetValue;


                    foreach (var reward in ac.rewards)
                        RewardPlayerForAchievement(reward);

                    OnAchievementComplete?.Invoke(ac.type);
                    break;
                }
            }
        }
    }

    void RewardPlayerForAchievement(AchievementReward reward)
    {
        switch (reward.type)
        {
            case RewardType.RewardDia:
                Managers.PlayerData.IncreaseDia(reward.integerParam);
                break;
            case RewardType.RewardCoins:
                Managers.PlayerData.IncreaseCoins(reward.integerParam);
                break;
            case RewardType.RewardStatus:
                Managers.Status.ApplyAchievementRewardStatus(reward);
                break;
            default:
                throw new System.ArgumentException($"Unknown AchievementRewardType {reward.type}");
        }
    }

    public void SetAchievementValueByTargetType(AchievementTargetType targetType, int value,
        ElementType elementType = ElementType.Energy, SummonType summonType = SummonType.Weapon)
    {
        var typeAchievements = _pendingAchievements.Values
        .SelectMany(list => list)
        .Where(achievement =>
            achievement.target.type == targetType &&
            ((targetType == AchievementTargetType.DefeatEnemies && achievement.target.elementType == elementType) ||
            (targetType == AchievementTargetType.StageClear) ||
            (targetType == AchievementTargetType.Summon && achievement.target.summonType == summonType)))
        .ToList();

        foreach (Achievement achievement in typeAchievements)
        {
            achievement.target.progressValue += value;
            if (achievement.target.progressValue >= achievement.target.targetValue)
                OnAchievementCompletable?.Invoke(achievement.type);
        }
    }

    public void SetAllAchievementTypeCompletable()
    {
        foreach(var pendingachievements in _pendingAchievements)
        {
            var typeAchievements = pendingachievements.Value.ToList();
            foreach(var achievement in typeAchievements)
            {
                if (achievement.target.progressValue >= achievement.target.targetValue)
                    OnAchievementCompletable?.Invoke(achievement.type);
            }
        }
    }

    public List<Achievement> GetAchievementsByCompleted(bool completed, AchievementType type)
    {
        if (completed)
        {
            if (_completedAchievements.TryGetValue(type, out var achievements))
                return achievements;
            return null;
        }
        else
        {
            if (_pendingAchievements.TryGetValue(type, out var achievements))
                return achievements;
            return null;
        }
    }

    public List<AchievementData> ToData()
    {
        List<Achievement> allAchievements = _completedAchievements.Values.SelectMany(list => list)
            .Concat(_pendingAchievements.Values.SelectMany(list => list)).ToList();

        return allAchievements.Select(achieve => achieve.ToData()).ToList();
    }

    public void Clear()
    {
        OnAchievementCompletable = null;
        OnAchievementComplete = null;
    }
}
