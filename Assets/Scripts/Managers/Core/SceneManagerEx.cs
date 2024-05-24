using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerEx : MonoBehaviour
{
    public BaseScene CurrentScene { get; set; }

    /// <summary>
    /// ���� Scene�� Ŭ�����ϰ� type�� �´� Scene�� ���������� �ε��մϴ�.
    /// </summary>
    public void LoadScene(Define.Scene type)
    {
        Managers.Clear();
        SceneManager.LoadScene(GetSceneName(type));
    }

    public void LoadSceneWithLoadingScene(Define.Scene type)
    {
        Managers.Clear();
        StartCoroutine(CoLoadGameAsync(type));
    }

    public IEnumerator CoLoadGameAsync(Define.Scene type)
    {
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
