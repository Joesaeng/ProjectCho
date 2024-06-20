using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Language
{
    public static bool isEng = false;
    public static string GetLanguage(string key)
    {
        switch (Managers.PlayerData.GameLanguage)
        {
            case Define.GameLanguage.English:
                return Managers.Data.LanguageDataDict[key].eng;
            case Define.GameLanguage.Korean:
                return Managers.Data.LanguageDataDict[key].kr;
            default:
                return Managers.Data.LanguageDataDict[key].kr;
        }

    }
}
