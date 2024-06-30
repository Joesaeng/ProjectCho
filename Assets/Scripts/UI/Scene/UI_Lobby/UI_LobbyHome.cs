using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_LobbyHome : UI_Base
{
    enum Objects
    {
        Button_NextStage,
        Button_PrevStage,
        Image_PlayBlocker,
        Image_StageNum,
        Text_StageNum,
        Button_Play,
        Text_Play,
        Panel_SpawnMonster,
        Text_WaveInfo,
    }
    int _curStage = 0;

    TextMeshProUGUI _stageNum;

    UI_SpawnMonsterItem[] _spawnMonsters;

    Button _prevStageButton;
    Button _nextStageButton;

    public override void Init()
    {
        Bind<GameObject>(typeof(Objects));

        _stageNum = GetObject((int)Objects.Text_StageNum).GetComponent<TextMeshProUGUI>();
        _stageNum.text = $"Stage {_curStage + 1}";

        _prevStageButton = GetObject((int)Objects.Button_PrevStage).GetComponent<Button>();
        _nextStageButton = GetObject((int)Objects.Button_NextStage).GetComponent<Button>();
        _prevStageButton.gameObject.AddUIEvent(ClickedPrevStageButton);
        _nextStageButton.gameObject.AddUIEvent(ClickedNextStageButton);

        GetObject((int)Objects.Button_Play).AddUIEvent(ClickedPlayButton);
        GetObject((int)Objects.Text_Play).GetComponent<TextMeshProUGUI>().text
            = Language.GetLanguage("Play");

        SetSpawnMonstersItem();
        SetLobbyHome();
    }
    void ClickedPrevStageButton(PointerEventData data)
    {
        if (_curStage > 0)
        {
            _curStage -= 1;
            Managers.Sound.Play("ui_click3");
            SetLobbyHome();
        }
    }

    void ClickedNextStageButton(PointerEventData data)
    {
        if (_curStage + 1 < Managers.Data.StageDataDict.Count)
        {
            _curStage += 1;
            Managers.Sound.Play("ui_click3");
            SetLobbyHome();
        }
    }

    void ClickedPlayButton(PointerEventData data)
    {
        LobbySceneManager.Instance.PlayGame(_curStage);
    }

    void SetLobbyHome()
    {
        _stageNum.text = $"Stage {_curStage + 1}";
        bool availablePlay = !Managers.PlayerData.StageClearList.Contains(_curStage-1);
        if (_curStage > 0)
            GetObject((int)Objects.Image_PlayBlocker).SetActive(availablePlay);
        else
            GetObject((int)Objects.Image_PlayBlocker).SetActive(false);
        SetPrevAndNextStageButton();
        SetSpawnMonsters();
        SetWaveInfo();
    }

    void SetWaveInfo()
    {
        TextMeshProUGUI waveInfoText = GetObject((int)Objects.Text_WaveInfo).GetComponent<TextMeshProUGUI>();
        string waveNum = $"<color=#FF0000><b>{Managers.Data.StageDataDict[_curStage].waveDatas.Count}</color></b> {Language.GetLanguage("Wave")}";
        int monstersCount = Managers.Data.StageDataDict[_curStage].waveDatas.Select(data => data.spawnEnemyCount).Sum();
        string monsterCountString = $"<color=#FF0000><b>{monstersCount}</color></b> {Language.GetLanguage("Monsters")}";

        waveInfoText.text = $"{waveNum} \n {monsterCountString}";
    }

    void SetPrevAndNextStageButton()
    {
        if (_curStage == 0)
            _prevStageButton.gameObject.SetActive(false);
        else
            _prevStageButton.gameObject.SetActive(true);
        if (_curStage + 1 < Managers.Data.StageDataDict.Count)
            _nextStageButton.gameObject.SetActive(true);
        else
            _nextStageButton.gameObject.SetActive(false);
    }

    void SetSpawnMonstersItem()
    {
        _spawnMonsters = new UI_SpawnMonsterItem[3];
        Transform tfSpawnMonster = GetObject((int)Objects.Panel_SpawnMonster).transform;
        for (int i = 0; i < tfSpawnMonster.childCount; ++i)
        {
            _spawnMonsters[i] = tfSpawnMonster.GetChild(i).GetComponent<UI_SpawnMonsterItem>();
            _spawnMonsters[i].Init();
        }

        SetSpawnMonsters();
    }

    void SetSpawnMonsters()
    {
        for (int i = 0; i < _spawnMonsters.Length; ++i)
        {
            _spawnMonsters[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < Managers.Data.StageDataDict[_curStage].stageEnemysId.Count; ++i)
        {
            if (i >= _spawnMonsters.Length)
            {
                Debug.LogError("현재 스테이지의 소환하는 몬스터 종류가 정해진 개수를 초과하였습니다");
                return;
            }
            _spawnMonsters[i].gameObject.SetActive(true);
            _spawnMonsters[i].SetSpawnMonster(Managers.Data.StageDataDict[_curStage].stageEnemysId[i]);
        }
    }

    public void ShowSpawnMonsters()
    {
        for (int i = 0; i < _spawnMonsters.Length; ++i)
        {
            _spawnMonsters[i].gameObject.SetActive(true);
        }
    }

    public void HideSpawnMonsters()
    {
        for (int i = 0; i < _spawnMonsters.Length; ++i)
        {
            _spawnMonsters[i].gameObject.SetActive(false);
        }
    }
}
