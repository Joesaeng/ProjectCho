using System;

// Json ������ ���� ����
namespace Data
{
    #region PlayerData

    // �÷��̾��� ���� ���� ��ü
    [Serializable]
    public class PlayerData
    {
        public bool     beginner        = true;
        public int      gameLanguage    = (int)Define.GameLanguage.English;
        public float    bgmVolume       = 1f;
        public float    sfxVolume       = 1f;
        public bool     bgmOn           = true;
        public bool     sfxOn           = true;
    }

    #endregion
}

