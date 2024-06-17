using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public abstract class UI_Base : MonoBehaviour
{
    protected Dictionary<Type, UnityEngine.Object[]> _objects = new();

    public abstract void Init();

    // private void Start()
    // {
    //     Init();
    // }

    protected void Bind<T>(Type type) where T : UnityEngine.Object
    {
        string[] names = Enum.GetNames(type);
        UnityEngine.Object[] objects = new UnityEngine.Object[names.Length];
            _objects.Add(typeof(T), objects);


        for (int i = 0; i < names.Length; ++i)
        {
            if (typeof(T) == typeof(GameObject))
                objects[i] = Util.FindChild(gameObject, names[i], true);
            else
                objects[i] = Util.FindChild<T>(gameObject, names[i], true);

            if (objects[i] == null)
                Debug.Log($"Failed to bind [{names[i]}]");
        }
    }

    protected T Get<T>(int index) where T : UnityEngine.Object
    {
        if (_objects.TryGetValue(typeof(T), out UnityEngine.Object[] objects) == false)
            return null;

        return objects[index] as T;
    }

    protected T[] Gets<T>() where T : UnityEngine.Object
    {
        if (_objects.TryGetValue(typeof(T), out UnityEngine.Object[] objects) == false)
            return null;
        T[] rets = objects.Cast<T>().ToArray();
        return rets;
    }

    protected GameObject GetObject(int index) { return Get<GameObject>(index); }
    protected TextMeshProUGUI GetText(int index) { return Get<TextMeshProUGUI>(index); }
    protected Button GetButton(int index) { return Get<Button>(index); }
    protected Image GetImage(int index) { return Get<Image>(index); }

    /// <summary>
    /// UI GameObject에 UIEvent를 Bind 바인드
    /// </summary>
    public static void BindEvent(GameObject go, Action<PointerEventData> action, Define.UIEvent type = Define.UIEvent.Click)
    {
        UI_EventHandler evt = go.GetOrAddComponent<UI_EventHandler>();

        switch (type)
        {
            case Define.UIEvent.Click:
                evt.OnClickHandler -= action;
                evt.OnClickHandler += action;
                break;
            case Define.UIEvent.Drag:
                evt.OnDragHandler -= action;
                evt.OnDragHandler += action;
                break;
        }
    }

    public static void BindEvent<T>(GameObject go, Action<T, PointerEventData> action, T value, Define.UIEvent type = Define.UIEvent.Click)
    {
        UI_EventHandler evt = go.GetOrAddComponent<UI_EventHandler>();

        switch (type)
        {
            case Define.UIEvent.Click:
                evt.OnClickHandler -= (data) => action(value, data);
                evt.OnClickHandler += (data) => action(value, data);
                break;
            case Define.UIEvent.Drag:
                evt.OnDragHandler -= (data) => action(value, data);
                evt.OnDragHandler += (data) => action(value, data);
                break;
        }
    }

    public static void RemoveEvent(GameObject go)
    {
        go.GetOrAddComponent<UI_EventHandler>().OnClickHandler = null;
        go.GetOrAddComponent<UI_EventHandler>().OnDragHandler = null;
    }

    // public abstract void OnChangeLanguage();

    /// <summary>
    /// to 까지 커지고 release로 돌아옵니다
    /// </summary>
    public void ButtonClickEffect(RectTransform rectTransform,float to = 1.4f,float release = 1f)
    {
        LeanTween.scale(rectTransform, Vector3.one * to, 0.2f).setEaseOutQuart().setOnComplete(() =>
        {
            LeanTween.scale(rectTransform, Vector3.one * release, 0.2f).setEaseOutQuart();
        });
    }
}
