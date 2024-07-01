using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ScreenShot : MonoBehaviour
{
    static int i = 0;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            string folderPath = Application.dataPath + "/ScreenShot";
            string filePath = $"{folderPath}/shot{i}.png";
            StartCoroutine(TakeScreenshotAndSave(folderPath, filePath));
            i++;
        }
    }
    public IEnumerator TakeScreenshotAndSave(string folderPath, string filePath)
    {
        yield return new WaitForEndOfFrame();
        Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        Rect area = new Rect(0f,0f,Screen.width, Screen.height);
        screenshot.ReadPixels(area, 0, 0);

        // 폴더가 존재하지 않으면 새로 생성
        if (Directory.Exists(folderPath) == false)
        {
            Directory.CreateDirectory(folderPath);
        }

        // 스크린샷 저장
        File.WriteAllBytes(filePath, screenshot.EncodeToPNG());
        Debug.Log($"Screenshot save to {filePath}");
        Destroy(screenshot);
    }

}
