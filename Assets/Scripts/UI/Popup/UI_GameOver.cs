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
    }

    enum Buttons
    {
        Button_Lobby,
        Button_AdsReward,
    }

    Transform _rewardsTf;
    public Action OnClickedLobby;
    public override void Init()
    {
        base.Init();
        Bind<GameObject>(typeof(Objects));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Button>(typeof(Buttons));

        _rewardsTf = GetObject((int)Objects.Obj_Rewards).transform;
        _defeatEnemiesUI = GetObject((int)Objects.UI_DefeatEnemies).GetComponent<UI_DefeatEnemies>();
        _defeatEnemiesUI.Init();

        GetText((int)Texts.Text_Lobby).text = Language.GetLanguage("Lobby");
        GetText((int)Texts.Text_AdsReward).text = Language.GetLanguage("AdsRewardx2");

        GetButton((int)Buttons.Button_Lobby).gameObject.AddUIEvent(ClickedLobby);
    }

    void ClickedLobby(PointerEventData data)
    {
        OnClickedLobby?.Invoke();
    }    

    public void SetGameoverUI(GameoverType type,Dictionary<RewardType,int> stageRewards, StageRewardData stageClearReward)
    {
        switch (type)
        {
            case GameoverType.Gameover:
                GetText((int)Texts.Text_Gameover).text = Language.GetLanguage("Lose");
                break;
            case GameoverType.Clear:
                GetText((int)Texts.Text_Gameover).text = Language.GetLanguage("Win");
                break;
        }
        foreach(var reward in stageRewards)
        {
            UI_GameoverReward ui = Managers.UI.MakeSubItem<UI_GameoverReward>(_rewardsTf);
            ui.SetReward(false,reward.Key,reward.Value);
        }
        if(stageClearReward != null)
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
