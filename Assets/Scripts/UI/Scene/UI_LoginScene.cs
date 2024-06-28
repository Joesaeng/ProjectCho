using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_LoginScene : UI_Scene
{
    enum Objects
    {
        Text_TouchToStart,
        Button_TouchToStart,
        Button_Guest,
        Panel_SelectLogin,
    }
    public override void Init()
    {
        Bind<GameObject>(typeof(Objects));

        GetObject((int)Objects.Panel_SelectLogin).SetActive(false);
        GetObject((int)Objects.Button_TouchToStart).AddUIEvent(CkickedLoginScene);

        GetObject((int)(Objects.Button_Guest)).AddUIEvent(ClickedGuestLogin);
    }

    void CkickedLoginScene(PointerEventData data)
    {
        GetObject((int)Objects.Text_TouchToStart).SetActive(false);
        GetObject((int)Objects.Button_TouchToStart).SetActive(false);

        // Firebase Auth에서 캐싱된 사용자가 있는지 확인
        if (FirebaseManager.Instance.CurrentUserId != null)
        {
            Debug.Log("Using cached user from FirebaseAuth: " + FirebaseManager.Instance.CurrentUserId);
            FirebaseManager.Instance.AvailableLoadPlayerData(FirebaseManager.Instance.CurrentUserId,(loadedPlayerData) =>
            {
                if (loadedPlayerData != null)
                {
                    FirebaseManager.Instance.LoadPlayerData(loadedPlayerData, Managers.PlayerData.OnPlayerDataLoadedToFirebase);
                }
                else
                    GetObject((int)Objects.Panel_SelectLogin).SetActive(true);
            });
        }
        else
            GetObject((int)Objects.Panel_SelectLogin).SetActive(true);
    }

    void ClickedGuestLogin(PointerEventData data)
    {
        FirebaseManager.Instance.SignOut(); // 기존 로그인 정보 지우기
        FirebaseManager.Instance.TryGuestLogin();
        Managers.PlayerData.NewPlayerLogin();
        Managers.Scene.LoadSceneWithLoadingScene(Define.Scene.Lobby);
    }
}
