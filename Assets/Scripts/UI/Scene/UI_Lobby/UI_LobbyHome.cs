using System.Collections;
using System.Collections.Generic;
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
        Image_Star0,
        Image_Star1,
        Image_Star2,
        Image_StageNum,
        Text_StageNum,
        Button_Play,
        Text_Play,
        Panel_SpawnMonster
    }
    int _curStage = 0;

    TextMeshProUGUI _stageNum;

    Image[] _stars;
    [SerializeField]
    Sprite[] _starSprite; // 0 : 빈별, 1 : 채워진 별

    UI_SpawnMonsterItem[] _spawnMonsters;

    Button _prevStageButton;
    Button _nextStageButton;

    public override void Init()
    {
        Bind<GameObject>(typeof(Objects));

        _stageNum = GetObject((int)Objects.Text_StageNum).GetComponent<TextMeshProUGUI>();
        _stageNum.text = $"Stage {_curStage + 1}";
        _stars = new Image[]
        {
            GetObject((int)Objects.Image_Star0).GetComponent<Image>(),
            GetObject((int)Objects.Image_Star1).GetComponent<Image>(),
            GetObject((int)Objects.Image_Star2).GetComponent<Image>(),
        };
        _prevStageButton = GetObject((int)Objects.Button_PrevStage).GetComponent<Button>();
        _nextStageButton = GetObject((int)Objects.Button_NextStage).GetComponent<Button>();
        _prevStageButton.gameObject.AddUIEvent(ClickedPrevStageButton);
        _nextStageButton.gameObject.AddUIEvent(ClickedNextStageButton);

        GetObject((int)Objects.Button_Play).AddUIEvent(ClickedPlayButton);

        SetPrevAndNextStageButton();
        SetStarsItem();
        SetSpawnMonstersItem();

    }
    void ClickedPrevStageButton(PointerEventData data)
    {
        if (_curStage > 0)
        {
            _curStage -= 1;
            SetLobbyHome();
        }
    }

    void ClickedNextStageButton(PointerEventData data)
    {
        if (_curStage + 1 < Managers.Data.StageDataDict.Count)
        {
            _curStage += 1;
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
        SetPrevAndNextStageButton();
        SetStarsItem();
        SetSpawnMonsters();
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

    void SetStarsItem()
    {
        for (int i = 0; i < _stars.Length; ++i)
        {
            // 플레이어 데이터의 ClearStage의 _curStage인덱스에서 
            // 클리어 값을 받아와서 별을 채우거나 비운 상태로 둔다
            _stars[i].sprite = _starSprite[0];
        }
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
}
