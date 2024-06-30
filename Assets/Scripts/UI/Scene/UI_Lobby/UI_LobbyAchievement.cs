using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_LobbyAchievement : UI_Base
{
    enum Objects
    {
        Content_Achievements
    }
    enum Buttons
    {
        Button_MainTab,
        Button_WeeklyTab,
        Button_DailyTab,
        Button_RepeatTab
    }
    enum Texts
    {
        Text_MainTab,
        Text_WeeklyTab,
        Text_DailyTab,
        Text_RepeatTab
    }
    enum Images
    {
        Image_MainCompletable,
        Image_WeeklyCompletable,
        Image_DailyCompletable,
        Image_RepeatCompletable
    }
    [SerializeField] Sprite[] _tabSprties; // 0 : active, 1 :deActive

    int _tabCount = 4;
    Button[] _tabButtons;
    Image[] _tabCompletables;

    Transform _achievementsTf;

    AchievementType _selectedAchievementType = AchievementType.Main;

    public override void Init()
    {
        Bind<GameObject>(typeof(Objects));
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));

        _tabButtons = new Button[_tabCount];
        _tabCompletables = new Image[_tabCount];

        for (int i = 0; i < _tabCount; ++i)
        {
            _tabButtons[i] = GetButton(i);
            _tabButtons[i].gameObject.AddUIEvent(ClickedTabButton, i);
            _tabCompletables[i] = GetImage(i);
            _tabCompletables[i].gameObject.SetActive(false);
        }

        _achievementsTf = GetObject((int)Objects.Content_Achievements).transform;
        for (int i = 0; i < _achievementsTf.childCount; ++i)
        {
            _achievementsTf.GetChild(i).GetComponent<UI_AchievementItem>().Init();
        }

        GetText((int)Texts.Text_MainTab).text = Language.GetLanguage("Main");
        GetText((int)Texts.Text_WeeklyTab).text = Language.GetLanguage("Weekly");
        GetText((int)Texts.Text_DailyTab).text = Language.GetLanguage("Daily");
        GetText((int)Texts.Text_RepeatTab).text = Language.GetLanguage("Repeat");
        SetTabs();
    }

    void ClickedTabButton(int index, PointerEventData data)
    {
        Managers.Sound.Play("ui_maintabclick");
        _selectedAchievementType = (AchievementType)index;
        SetTabs();
    }

    void SetTabs()
    {
        for(int i = 0; i < _tabCount; ++i)
        {
            if (i == (int)_selectedAchievementType)
                _tabButtons[i].GetComponent<Image>().sprite = _tabSprties[0];
            else
                _tabButtons[i].GetComponent<Image>().sprite = _tabSprties[1];
        }
        SetAchievements();
    }

    public bool AchievementCompleteListner(AchievementType type)
    {
        // 해당 업적 타입에 완료 가능한 업적이 있는지 확인
        var achievements = Managers.Achieve.GetAchievementsByCompleted(false,type) ?? Enumerable.Empty<Achievement>();

        bool hasCompletableAchievements = achievements
            .Any(ac => ac.target.progressValue >= ac.target.targetValue);

        // 이미지 오브젝트의 활성화 상태를 설정
        _tabCompletables[(int)type].gameObject.SetActive(hasCompletableAchievements);
        SetAchievements();

        return hasCompletableAchievements;
    }

    public void AchievementCompletableListner(AchievementType type)
    {
        _tabCompletables[(int)type].gameObject.SetActive(true);
    }

    public void SetAchievements()
    {
        List<Achievement> achievements = new();
        achievements.AddRange(Managers.Achieve.GetAchievementsByCompleted(false, _selectedAchievementType) 
            ?? Enumerable.Empty<Achievement>());
        achievements.AddRange(Managers.Achieve.GetAchievementsByCompleted(true, _selectedAchievementType) 
            ?? Enumerable.Empty<Achievement>());

        int index = 0;
        while (index < achievements.Count)
        {
            if (_achievementsTf.TryGetChild(index, out UI_AchievementItem achievementItem))
            {
                achievementItem.gameObject.SetActive(true);
                achievementItem.SetAchievement(achievements[index]);
            }
            else
            {
                MakeAchievementItem().SetAchievement(achievements[index]);
            }
            index++;
        }
        for(int i = index; i < _achievementsTf.childCount; ++i)
        {
            _achievementsTf.GetChild(i).gameObject.SetActive(false);
        }
    }

    UI_AchievementItem MakeAchievementItem()
    {
        UI_AchievementItem achievementItem = Managers.UI.MakeSubItem<UI_AchievementItem>(_achievementsTf);
        achievementItem.transform.localPosition = Vector3.zero;
        achievementItem.transform.localScale = Vector3.one;
        achievementItem.Init();
        return achievementItem;
    }
}
