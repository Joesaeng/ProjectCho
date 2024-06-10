using System.Collections;
using System.Collections.Generic;
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
        Button_EnergyAdd,
        Button_CoinAdd,
        Button_DiaAdd,

        // ==== 메뉴 버튼 ====
        Button_Home,
        Button_Magician,
        Button_Shop,
        Button_Achieve,

    }
    enum Texts
    {
        Text_EnergyValue,
        Text_CoinValue,
        Text_DiaValue,

        // ==== 메뉴 텍스트 ====
        Text_Home,
        Text_Magician,
        Text_Shop,
        Text_Achieve,
    }
    enum Images
    {
        // ==== 메뉴 이미지 ====
        Image_Home,
        Image_Magician,
        Image_Shop,
        Image_Achieve,
    }
    enum Objects
    {
        Slider_MenuTab,

        // ==== 메뉴 오브젝트 ====
        UI_LobbyHome,
        UI_LobbyMagician
    }

    // ======= 오브젝트 ========
    // - 메뉴탭
    Slider _menuTabSlider;
    bool _menuMove = false;
    ushort _selectedIndex = 0;
    Button[] _menuTabButtons;
    TextMeshProUGUI[] _menuTabTexts;
    Image[] _menuTabImages;

    // UI_LobbyHome ui_home;
    UI_Base[] _menuUis;


    public override void Init()
    {
        // base.Init();
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));
        Bind<GameObject>(typeof(Objects));

        #region 메뉴 탭 초기화
        _menuUis = new UI_Base[4];
        _menuUis[0] = GetObject((int)Objects.UI_LobbyHome).GetComponent<UI_LobbyHome>();
        _menuUis[0].Init();
        _menuUis[1] = GetObject((int)Objects.UI_LobbyMagician).GetComponent<UI_LobbyMagician>();
        _menuUis[1].Init();

        _menuTabButtons = new Button[]
        {
            GetButton((int)Buttons.Button_Home),
            GetButton((int)Buttons.Button_Magician),
            GetButton((int)Buttons.Button_Shop),
            GetButton((int)Buttons.Button_Achieve),
        };
        _menuTabTexts = new TextMeshProUGUI[]
        {
            GetText((int)Texts.Text_Home),
            GetText((int)Texts.Text_Magician),
            GetText((int)Texts.Text_Shop),
            GetText((int)Texts.Text_Achieve)

        };
        _menuTabImages = new Image[]
        {
            GetImage((int)Images.Image_Home),
            GetImage((int)Images.Image_Magician),
            GetImage((int)Images.Image_Shop),
            GetImage((int)Images.Image_Achieve),
        };
        for (ushort i = 0; i < _menuTabButtons.Length; i++)
        {
            _menuTabButtons[i].gameObject.AddUIEvent(ClickedMenuButtons, i);
        }
        _selectedIndex = 0;
        MoveMenuToIndex(_selectedIndex);
        #endregion

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

    #region 메뉴 탭 버튼 클릭
    void MoveMenuToIndex(ushort selectIndex)
    {
        _selectedIndex = selectIndex;
        ButtonClickEffect(_menuTabImages[selectIndex].rectTransform, release:1.2f);
        _menuMove = true;
        for (ushort i = 0; i < _menuTabButtons.Length; i++)
        {
            if (i == selectIndex)
            {
                _menuTabButtons[i].gameObject.GetComponent<Image>().color = new Color(1f,0.87f,0.46f);
                _menuTabTexts[i].enabled = false;
                if (_menuUis[i] != null)
                    _menuUis[i].gameObject.SetActive(true);
                
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

    void ClickedMenuButtons(ushort selectedIndex, PointerEventData data)
    {
        MoveMenuToIndex(selectedIndex);
        // Index에 맞는 메뉴 표시
    }

    #endregion
}
