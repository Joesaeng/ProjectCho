using Data;
using Define;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_GameOver : UI_Popup
{
    UI_DefeatEnemies _defeatEnemiesUI;

    enum Texts
    {
        Text_Gameover,
        Text_Lobby,
        Text_AdsReward,
    }

    enum Objects
    {
        Obj_Rewards,
        UI_DefeatEnemies,
        Image_BackGround
    }

    enum Buttons
    {
        Button_Lobby,
        Button_AdsReward,
    }

    RectTransform _backTf;
    Transform _rewardsTf;
    public Action OnClickedLobby;
    public Action OnClickedAds;
    public override void Init()
    {
        base.Init();
        Bind<GameObject>(typeof(Objects));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Button>(typeof(Buttons));

        _rewardsTf = GetObject((int)Objects.Obj_Rewards).transform;
        _defeatEnemiesUI = GetObject((int)Objects.UI_DefeatEnemies).GetComponent<UI_DefeatEnemies>();
        _defeatEnemiesUI.Init();

        _backTf = GetObject((int)Objects.Image_BackGround).GetComponent<RectTransform>();

        GetText((int)Texts.Text_Lobby).text = Language.GetLanguage("Lobby");
        GetText((int)Texts.Text_AdsReward).text = Language.GetLanguage("AdsRewardx2");

        GetButton((int)Buttons.Button_Lobby).gameObject.AddUIEvent(ClickedLobby);
        GetButton((int)Buttons.Button_AdsReward).gameObject.AddUIEvent((PointerEventData data)
            => Managers.Ads.ShowRewardedAd(() => OnClickedAds.Invoke()));
    }

    void ClickedLobby(PointerEventData data)
    {
        OnClickedLobby?.Invoke();
    }

    public void ShowGameOverUI()
    {
        gameObject.SetActive(true);
        _backTf.localScale = Vector3.one * 0.5f;

        LeanTween.scale(_backTf, Vector3.one, 0.5f).setEase(LeanTweenType.easeOutCirc).setIgnoreTimeScale(true);
    }

    public void SetGameoverUI(GameoverType type, Dictionary<RewardType, int> stageRewards, StageRewardData stageClearReward)
    {
        switch (type)
        {
            case GameoverType.Gameover:
                GetText((int)Texts.Text_Gameover).text = Language.GetLanguage("Lose");
                Managers.Sound.Play("ui_lose");
                break;
            case GameoverType.Clear:
                GetText((int)Texts.Text_Gameover).text = Language.GetLanguage("Win");
                Managers.Sound.Play("ui_win");
                break;
        }
        foreach (var reward in stageRewards)
        {
            UI_GameoverReward ui = Managers.UI.MakeSubItem<UI_GameoverReward>(_rewardsTf);
            ui.SetReward(false, reward.Key, reward.Value);
        }
        if (stageClearReward != null)
        {
            UI_GameoverReward ui = Managers.UI.MakeSubItem<UI_GameoverReward>(_rewardsTf);
            ui.SetReward(true, stageClearReward.type, stageClearReward.value);
        }
    }

    public void SetDefeatEnemies(Dictionary<ElementType, int> defeatEnemies)
    {
        _defeatEnemiesUI.SetDefeatEnemies(defeatEnemies);
    }
}
