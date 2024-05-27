using System.IO;
using UnityEngine;

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

    // 플레이어 데이터를 UTF-8로 인코딩하여 저장합니다
    public void SaveToJson()
    {
        if (File.Exists(_path))
            File.Delete(_path);

        string json = JsonUtility.ToJson(_playerData);

        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);

        string encodedJson = System.Convert.ToBase64String(bytes);

        File.WriteAllText(_path, encodedJson);

    }

    // UTF-8로 인코딩된 데이터를 디코딩하여 불러옵니다
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
