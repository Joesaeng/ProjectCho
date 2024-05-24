using System;

// Json 데이터 저장 형식
namespace Data
{
    #region PlayerData

    // 플레이어의 정보 저장 객체
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

