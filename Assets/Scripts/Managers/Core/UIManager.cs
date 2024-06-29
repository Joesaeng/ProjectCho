using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UIManager
{
    int _order = ConstantData.PopupUISortOrder; // UI들의 sort order

    Stack<UI_Popup> _popupStack = new Stack<UI_Popup>();
    UI_Scene _sceneUI = null;

    /// <summary>
    /// 외부에서 팝업UI가 켜질 때 본인 캔버스의 sortorder를 채워주는 함수
    /// </summary>
    public void SetCanvas(GameObject go, bool sort = true)
    {
        Canvas canvas = Util.GetOrAddComponent<Canvas>(go);
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = GameObject.Find("UI_Camera").GetComponent<Camera>();

        // Canvas가 중첩으로 있을 때, 부모의 sortingorder를 따라가지 않고 자신의 sortingorder를 가지게 함
        canvas.overrideSorting = true;

        if (sort)
        {
            canvas.sortingOrder = _order;
            _order++;
        }
        else
        {
            canvas.sortingOrder = ConstantData.SceneUISortOrder;
        }
    }

    public T ShowSceneUI<T>(string name = null) where T : UI_Scene
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        T sceneUI;
        if(GameObject.FindAnyObjectByType<T>() != null)
        {
            sceneUI = GameObject.FindAnyObjectByType<T>();
        }
        else
        {
            GameObject go = Managers.Resource.Instantiate($"UI/Scene/{name}");
            sceneUI = Util.GetOrAddComponent<T>(go);
        }
        _sceneUI = sceneUI;
        return sceneUI;
        
    }

    public T MakeWorldSpaceUI<T>(Transform parent = null, string name = null) where T : UI_Base
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate($"UI/WorldSpace/{name}");

        if (parent != null)
            go.transform.SetParent(parent);

        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        canvas.sortingOrder = ConstantData.WorldSpaceUISortOrder;

        T component = go.GetOrAddComponent<T>();

        return component;
    }

    public void CloseWorldSpaceUI(GameObject closeUI)
    {
        Managers.Resource.Destroy(closeUI);
    }

    public T MakeSubItem<T>(Transform parent = null, string name = null) where T : UI_Base
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate($"UI/SubItem/{name}");

        if (parent != null)
            go.transform.SetParent(parent);

        T component = go.GetOrAddComponent<T>();

        return component;
    }

    public T ShowPopupUI<T>(string name = null) where T : UI_Popup
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        T popup;
        if (GameObject.FindAnyObjectByType<T>() != null)
        {
            popup = GameObject.FindAnyObjectByType<T>();
        }
        else
        {
            GameObject go = Managers.Resource.Instantiate($"UI/Popup/{name}");
            popup = Util.GetOrAddComponent<T>(go);
        }

        _popupStack.Push(popup);

        return popup;
    }

    public void ClosePopupUI(UI_Popup popup)
    {
        if (_popupStack.Count == 0)
            return;

        if (_popupStack.Peek() != popup)
        {
            Debug.LogWarning("Close Popup Failed");
            return;
        }

        ClosePopupUI();
    }

    public void ClosePopupUI()
    {
        if (_popupStack.Count == 0)
            return;

        UI_Popup popup = _popupStack.Pop();
        if(popup != null)
            Managers.Resource.Destroy(popup.gameObject);
        popup = null;

        _order--;
    }

    public void CloseAllPopupUI()
    {
        while (_popupStack.Count > 0)
        {
            ClosePopupUI();
        }
    }

    public void Clear()
    {
        CloseAllPopupUI();
        if (_sceneUI != null)
        {
            _sceneUI = null;
        }
    }
}
