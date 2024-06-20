using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_GameoverReward : UI_Base
{
    [SerializeField] Sprite[] _rewardSprites;

    public override void Init()
    {
        
    }

    public void SetReward(bool firstClear, RewardType type, int value)
    {
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;

        TextMeshProUGUI firstClearText = Util.FindChild<TextMeshProUGUI>(gameObject,"Text_FirstClear");
        firstClearText.gameObject.SetActive(firstClear);
        firstClearText.text = Language.GetLanguage("FirstClear");

        Util.FindChild<Image>(gameObject,"Image_Reward").sprite = _rewardSprites[(int)type];
        Util.FindChild<TextMeshProUGUI>(gameObject,"Text_RewardValue").text = value.ToString();
    }
}
