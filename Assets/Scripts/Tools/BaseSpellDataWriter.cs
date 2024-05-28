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
    public SpellType spellType;
    public MagicianAnim animType;
    public ElementType elementType;
    public string spellName;
    public float spellDamage;
    public float spellDelay;
    public float spellRange;
    public float spellSpeed;
    public float spellDuration;

    public void WriteData()
    {
        BaseSpellData newData = new BaseSpellData()
        {
            effectId = effectId,
            spellType = spellType,
            animType = animType,
            elementType = elementType,
            spellName = spellName,
            spellDamage = spellDamage,
            spellDelay = spellDelay,
            spellRange = spellRange, 
            spellSpeed = spellSpeed, 
            spellDuration = spellDuration
        };
        JsonDataWriter.WriteData(jsonpath, newData);
    }
}