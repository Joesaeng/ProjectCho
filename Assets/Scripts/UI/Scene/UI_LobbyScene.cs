using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_LobbyScene : UI_Scene
{
    enum Buttons
    {
        Button_Setting,

        // ==== 메뉴 버튼 ====
        Button_Home,
        Button_Magician,
        Button_Shop,
        Button_Achievement,

    }
    enum Texts
    {
        Text_CoinAmount,
        Text_DiaAmount,

        // ==== 메뉴 텍스트 ====
        Text_Home,
        Text_Magician,
        Text_Shop,
        Text_Achievement,
    }
    enum Images
    {
        // ==== 메뉴 이미지 ====
        Image_Home,
        Image_Magician,
        Image_Shop,
        Image_Achievement,

        Image_AchieveCompletable
    }
    enum Objects
    {
        // ==== 메뉴 오브젝트 ====
        UI_LobbyHome,
        UI_LobbyMagician,
        UI_LobbyShop,
        UI_LobbyAchievement,

        Slider_MenuTab,
    }

    // ======= 오브젝트 ========
    // - 메뉴탭
    int _tabCount = 4;
    Slider _menuTabSlider;
    bool _menuMove = false;
    int _selectedIndex = 0;
    Button[] _menuTabButtons;
    TextMeshProUGUI[] _menuTabTexts;
    Image[] _menuTabImages;

    // UI_LobbyHome ui_home;
    UI_Base[] _menuUis;

    TextMeshProUGUI _coinAmountText;
    TextMeshProUGUI _diaAmountText;

    public override void Init()
    {
        // base.Init();
        GetComponent<Canvas>().worldCamera = GameObject.Find("UI_Camera").GetComponent<Camera>();
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));
        Bind<GameObject>(typeof(Objects));

        #region 메뉴 탭 초기화
        _menuUis = new UI_Base[_tabCount];
        _menuUis[0] = GetObject((int)Objects.UI_LobbyHome).GetComponent<UI_LobbyHome>();
        _menuUis[0].Init();
        _menuUis[1] = GetObject((int)Objects.UI_LobbyMagician).GetComponent<UI_LobbyMagician>();
        _menuUis[1].Init();
        _menuUis[2] = GetObject((int)Objects.UI_LobbyShop).GetComponent<UI_LobbyShop>();
        _menuUis[2].Init();
        _menuUis[3] = GetObject((int)Objects.UI_LobbyAchievement).GetComponent<UI_LobbyAchievement>();
        _menuUis[3].Init();

        _menuTabButtons = new Button[]
        {
            GetButton((int)Buttons.Button_Home),
            GetButton((int)Buttons.Button_Magician),
            GetButton((int)Buttons.Button_Shop),
            GetButton((int)Buttons.Button_Achievement),
        };
        _menuTabTexts = new TextMeshProUGUI[]
        {
            GetText((int)Texts.Text_Home),
            GetText((int)Texts.Text_Magician),
            GetText((int)Texts.Text_Shop),
            GetText((int)Texts.Text_Achievement)

        };
        _menuTabImages = new Image[]
        {
            GetImage((int)Images.Image_Home),
            GetImage((int)Images.Image_Magician),
            GetImage((int)Images.Image_Shop),
            GetImage((int)Images.Image_Achievement),
        };
        for (int i = 0; i < _tabCount; i++)
        {
            _menuTabButtons[i].gameObject.AddUIEvent(ClickedMenuButtons, i);
        }
        _selectedIndex = 0;
        MoveMenuToIndex(_selectedIndex);
        #endregion

        _coinAmountText = GetText((int)Texts.Text_CoinAmount);
        _diaAmountText = GetText((int)Texts.Text_DiaAmount);
        Managers.PlayerData.OnChangeCoinAmount += ChangeCoinAmount;
        Managers.PlayerData.OnChangeDiaAmount += ChangeDiaAmount;

        ChangeCoinAmount(Managers.PlayerData.CoinAmount);
        ChangeDiaAmount(Managers.PlayerData.DiaAmount);

        Managers.Achieve.OnAchievementCompletable += AchievementCompletableListner;
        Managers.Achieve.OnAchievementComplete += AchievementCompleteListner;

        GetImage((int)Images.Image_AchieveCompletable).gameObject.SetActive(false);

        _menuTabSlider = GetObject((int)Objects.Slider_MenuTab).GetComponent<Slider>();
    }
    private void LateUpdate()
    {
        if (_menuMove)
        {
            _menuTabSlider.value = Mathf.Lerp(_menuTabSlider.value, _selectedIndex, Time.deltaTime * 15);
            if (Mathf.Abs(_menuTabSlider.value - _selectedIndex) < 0.001f)
                _menuMove = false;
        }
    }

    #region 메뉴 탭
    void MoveMenuToIndex(int selectedIndex)
    {
        _selectedIndex = selectedIndex;
        ButtonClickEffect(_menuTabImages[selectedIndex].rectTransform, release:1.2f);
        _menuMove = true;
        for (int i = 0; i < _tabCount; i++)
        {
            if (i == selectedIndex)
            {
                _menuTabButtons[i].gameObject.GetComponent<Image>().color = new Color(1f,0.87f,0.46f);
                _menuTabTexts[i].enabled = false;
                if (_menuUis[i] != null)
                    _menuUis[i].gameObject.SetActive(true);
                if (_menuUis[i].TryGetComponent<UI_LobbyAchievement>(out var achievement))
                    achievement.SetAchievements();
                
            }
            else
            {
                _menuTabButtons[i].gameObject.GetComponent<Image>().color = Color.white;
                _menuTabTexts[i].enabled = true;
                LeanTween.scale(_menuTabImages[i].rectTransform, Vector3.one, 0.2f).setEaseOutQuart();
                if (_menuUis[i] != null)
                    _menuUis[i].gameObject.SetActive(false);
            }
        }
    }

    void ClickedMenuButtons(int selectedIndex, PointerEventData data)
    {
        MoveMenuToIndex(selectedIndex);
        // Index에 맞는 메뉴 표시
    }

    #endregion

    void AchievementCompleteListner(AchievementType type)
    {
        UI_LobbyAchievement achievement = _menuUis[(int)Objects.UI_LobbyAchievement] as UI_LobbyAchievement;

        GetImage((int)Images.Image_AchieveCompletable).gameObject
            .SetActive(achievement.AchievementCompleteListner(type));
    }

    void AchievementCompletableListner(AchievementType type)
    {
        UI_LobbyAchievement achievement = _menuUis[(int)Objects.UI_LobbyAchievement] as UI_LobbyAchievement;
        achievement.AchievementCompleteListner(type);

        GetImage((int)Images.Image_AchieveCompletable).gameObject.SetActive(true);
    }

    void ChangeCoinAmount(int value)
    {
        _coinAmountText.text = value.ToString();
    }

    void ChangeDiaAmount(int value)
    {
        _diaAmountText.text = value.ToString();
    }
}
