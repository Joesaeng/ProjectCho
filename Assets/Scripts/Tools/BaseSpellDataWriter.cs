using Data;
using Define;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BaseSpellDataWriter : MonoBehaviour
{
    string jsonpath = "Resources/Data/BaseSpellData.json";

    public int effectId;
    public SpellBehaviorType spellType;
    public SpellRarity spellRarity;
    public MagicianAnim animType;
    public ElementType elementType;
    public string spellName;
    public List<BaseSpellDataByLevel> spellDataByLevel;
    public float spellRange;
    public float spellSpeed;
    public float spellSize;
    public int pierceCount;

    public int integerParam1;
    public int integerParam2;
    public float floatParam1;
    public float floatParam2;

    public void WriteData()
    {
        BaseSpellData newData = new BaseSpellData()
        {
            effectId = effectId,
            spellBehaviorType = spellType,
            spellRarity = spellRarity,
            animType = animType,
            elementType = elementType,
            spellName = spellName,
            spellDataByLevel = spellDataByLevel,
            spellRange = spellRange, 
            spellSpeed = spellSpeed, 
            spellSize = spellSize,
            pierceCount = pierceCount,
            integerParam1 = integerParam1,
            integerParam2 = integerParam2,
            floatParam1 = floatParam1,
            floatParam2 = floatParam2,
};
        JsonDataWriter.WriteData(jsonpath, newData);
    }
}