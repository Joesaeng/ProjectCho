using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class UI_AchievementItem : UI_Base
{
    enum Texts
    {
        Text_AchievementName,
        Text_AchievementValue,
        Text_AchievementReward,
        Text_Claim,
        Text_Completed,
    }
    enum Buttons
    {
        Button_Claim,
    }
    enum Images
    {
        Image_Completed
    }

    Achievement _achievement;
    Slider _valueSlider;

    public override void Init()
    {
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));

        GetButton((int)Buttons.Button_Claim).gameObject.AddUIEvent(ClickedCliamButton);

        GetText((int)Texts.Text_Claim).text = Language.GetLanguage("Claim");
        GetText((int)Texts.Text_Completed).text = Language.GetLanguage("Clear");

        if (_valueSlider == null)
            _valueSlider = GetComponentInChildren<Slider>();
    }

    void ClickedCliamButton(PointerEventData data)
    {
        Managers.Achieve.CompleteAchievement(_achievement);
    }

    public void SetAchievement(Achievement achievement)
    {
        _achievement = achievement;
        if (_achievement.isCompleted)
        {
            SetCompletedAchievement();
            return;
        }

        GetText((int)Texts.Text_AchievementName).text = GetAchievementName(_achievement.target);
        GetText((int)Texts.Text_AchievementReward).text = GetAchievementRewardToString(_achievement.rewards);
        GetText((int)Texts.Text_AchievementValue).text =
            $"[{_achievement.target.progressValue} / {_achievement.target.targetValue}]";
        float completionRatio = (float)_achievement.target.progressValue / _achievement.target.targetValue;
        _valueSlider.value = completionRatio;

        GetButton((int)Buttons.Button_Claim).interactable = completionRatio >= 1;
        GetImage((int)Images.Image_Completed).gameObject.SetActive(false);
    }

    void SetCompletedAchievement()
    {
        GetImage((int)Images.Image_Completed).gameObject.SetActive(true);
        GetButton((int)Buttons.Button_Claim).interactable = false;
    }

    string GetAchievementName(AchievementTarget target)
    {
        return target.type switch
        {
            AchievementTargetType.DefeatEnemies =>
                string.Format(
                Language.GetLanguage(target.type.ToString()),
                Language.GetLanguage(target.elementType.ToString())),
            AchievementTargetType.StageClear =>
                Language.GetLanguage(target.type.ToString()),
            AchievementTargetType.Summon =>
                string.Format(
                Language.GetLanguage(target.type.ToString()),
                Language.GetLanguage(target.summonType.ToString())),
            _ => throw new System.ArgumentException($"Unknown AchievementTargetType : {target.type}")
        };
    }

    string GetAchievementRewardToString(List<AchievementReward> rewards)
    {
        string ret = "";
        for (int i = 0; i < rewards.Count; i++)
        {
            var reward = rewards[i];
            bool isLast = (i == rewards.Count - 1);

            switch (reward.type)
            {
                case AchievementRewardType.RewardDia:
                case AchievementRewardType.RewardCoins:
                    ret += string.Format(Language.GetLanguage(reward.type.ToString()), reward.integerParam);
                    break;
                case AchievementRewardType.RewardStatus:
                    ret += string.Format(Language.GetLanguage(reward.type.ToString()),
                        Language.GetLanguage(reward.statusType.ToString()), reward.floatParam * 100);
                    break;
            }

            if (!isLast)
            {
                ret += $" {Language.GetLanguage("And")} ";
            }
        }

        return ret;
    }
}
