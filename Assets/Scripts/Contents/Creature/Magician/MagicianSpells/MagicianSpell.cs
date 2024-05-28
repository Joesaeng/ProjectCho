using Data;
using Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MagicianSpell
{
    public abstract void Init(BaseSpellData baseSpellData);

    public abstract void UseSpell(IHitable target);
}
