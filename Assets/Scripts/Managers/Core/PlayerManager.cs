using System.IO;
using UnityEngine;

// 플레이어의 데이터를 관리, 저장 및 로드 담당
public class PlayerManager
{
    Data.PlayerData _playerData;
    public Data.PlayerData Data
    {
        get
        {
            return _playerData;
        }
        set { _playerData = value; }
    }

    string _path;

    public void Init()
    {
        _path = Application.persistentDataPath + "/PlayerData";
        LoadFromJson();
    }

    // 플레이어 데이터를 UTF-8 인코딩하여 저장
    public void SaveToJson()
    {
        if (File.Exists(_path))
            File.Delete(_path);

        string json = JsonUtility.ToJson(_playerData);

        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);

        string encodedJson = System.Convert.ToBase64String(bytes);

        File.WriteAllText(_path, encodedJson);

    }

    // UTF-8 데이터로 저장된 플레이어 데이터를 읽어온 후 디코딩
    public void LoadFromJson()
    {
        if (!File.Exists(_path))
        {
            _playerData = new Data.PlayerData();

            SaveToJson();
        }

        string jsonData = File.ReadAllText(_path);

        byte[] bytes = System.Convert.FromBase64String(jsonData);

        string decodedJson = System.Text.Encoding.UTF8.GetString(bytes);

        _playerData = JsonUtility.FromJson<Data.PlayerData>(decodedJson);
    }
}
