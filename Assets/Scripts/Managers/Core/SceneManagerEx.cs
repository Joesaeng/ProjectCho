using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerEx : MonoBehaviour
{
    public BaseScene CurrentScene { get; set; }

    /// <summary>
    /// 현재 Scene을 클리어하고 type에 맞는 Scene을 동기적으로 로드합니다.
    /// </summary>
    public void LoadScene(Define.Scene type)
    {
        Managers.Clear();
        SceneManager.LoadScene(GetSceneName(type));
    }

    Define.Scene _loadSceneType;
    public void LoadSceneWithLoadingScene(Define.Scene type)
    {
        Managers.Clear();
        UI_Loading loading = Managers.UI.ShowPopupUI<UI_Loading>();
        _loadSceneType = type;
        loading.Init();
        loading.OnCompleteLoadingUI += CompleteShowLoadingListener;
        loading.ShowLoadingUI();
    }

    void CompleteShowLoadingListener()
    {
        StartCoroutine(CoLoadGameAsync(_loadSceneType));
    }

    public IEnumerator CoLoadGameAsync(Define.Scene type)
    {
        Managers.PlayerData.SaveToFirebase();
        AsyncOperation ao = SceneManager.LoadSceneAsync(GetSceneName(type));
        ao.allowSceneActivation = false;

        while (!ao.isDone)
        {
            yield return null;
            if (ao.progress < 0.9f)
            {

            }
            else
            {
                ao.allowSceneActivation = true;
            }
        }
        // UI_Loading loading = Managers.UI.ShowPopupUI<UI_Loading>();
        // loading.Init();
        // loading.OnCompleteLoadingUI += () => Managers.UI.ClosePopupUI(loading);
    }

    string GetSceneName(Define.Scene type)
    {
        return System.Enum.GetName(typeof(Define.Scene), type);
    }

    public void Clear()
    {
        CurrentScene.Clear();
    }
}
