using System.IO;
using UnityEngine;

// �÷��̾��� �����͸� ����, ���� �� �ε� ���
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

    // �÷��̾� �����͸� UTF-8 ���ڵ��Ͽ� ����
    public void SaveToJson()
    {
        if (File.Exists(_path))
            File.Delete(_path);

        string json = JsonUtility.ToJson(_playerData);

        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);

        string encodedJson = System.Convert.ToBase64String(bytes);

        File.WriteAllText(_path, encodedJson);

    }

    // UTF-8 �����ͷ� ����� �÷��̾� �����͸� �о�� �� ���ڵ�
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
