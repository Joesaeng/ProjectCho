using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_DefenseScene : UI_Scene
{
    TextMeshProUGUI _playerHpText;

    enum Texts
    {
        Text_PlayerHp,
    }

    public override void Init()
    {
        base.Init();
        // UI 바인드
        Bind<TextMeshProUGUI>(typeof(Texts));


        // 캐싱
        _playerHpText = GetText((int)Texts.Text_PlayerHp);

        #region 이벤트 바인드
        DefenseSceneManager.Instance.OnUpdatePlayerHp -= UpdatePlayerHpListner;
        DefenseSceneManager.Instance.OnUpdatePlayerHp += UpdatePlayerHpListner;
        #endregion
    }

    private void UpdatePlayerHpListner(int curHp,int maxHp)
    {
        if(_playerHpText.gameObject.activeSelf == false)
        {
            _playerHpText.gameObject.SetActive(true);
        }
        _playerHpText.text = $"{curHp} / {maxHp}";
    }
}
