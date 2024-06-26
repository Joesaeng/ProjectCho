using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickEffectManager : MonoBehaviour
{
    public GameObject clickEffectPrefab; // 클릭 이펙트 프리팹
    public Canvas uiCanvas; // UI Canvas

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 마우스 왼쪽 버튼 클릭 체크
        {
            ShowClickEffect();
        }
    }

    void ShowClickEffect()
    {
        // 클릭한 위치를 가져옵니다.
        Vector3 clickPosition = Input.mousePosition;

        // UI 카메라가 있는지 확인하고 없으면 메인 카메라 사용
        Camera uiCamera = uiCanvas.worldCamera != null ? uiCanvas.worldCamera : Camera.main;

        // UI 캔버스의 RectTransform을 사용하여 올바른 UI 위치 계산
        RectTransformUtility.ScreenPointToWorldPointInRectangle(uiCanvas.transform as RectTransform, clickPosition, uiCamera, out Vector3 uiPosition);

        // 클릭 이펙트를 생성하고 캔버스의 자식으로 설정합니다.
        // GameObject clickEffect = Instantiate(clickEffectPrefab, uiCanvas.transform);
        GameObject clickEffect = Managers.Resource.Instantiate("UI_ClickEffect", uiCanvas.transform);
        clickEffect.transform.position = uiPosition;

        // 클릭 이펙트를 일정 시간 후 제거합니다.
        // Destroy(clickEffect, 1f); // 1초 후 이펙트 제거
        Managers.Resource.Destroy(clickEffect,1f);
    }
}