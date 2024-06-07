using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Language
{
    public static bool isEng = true;
    public static string GetLanguage(string key)
    {
        if (isEng)
            return Managers.Data.LanguageDataDict[key].eng;
        else
            return Managers.Data.LanguageDataDict[key].kr;
    }
}
