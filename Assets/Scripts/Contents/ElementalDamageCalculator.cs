using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ElementalDamageCalculator
{
    // private static readonly float[,] damageMultiplier = {
    //     //         En     Fi     Wa     Li     Ea     Ai     Li     Da
    //     /* En */ { 1.0f,  1.0f,  1.0f,  1.0f,  1.0f,  1.0f,  1.0f,  1.0f  },
    //     /* Fi */ { 0.75f, 0.75f, 0.5f,  0.75f, 0.75f, 2.0f,  0.75f, 0.75f },
    //     /* Wa */ { 0.75f, 2.0f,  0.75f, 0.5f,  0.75f, 0.75f, 0.75f, 0.75f },
    //     /* Li */ { 0.75f, 0.75f, 2.0f,  0.75f, 0.5f,  0.75f, 0.75f, 0.75f },
    //     /* Ea */ { 0.75f, 0.75f, 0.75f, 2.0f,  0.75f, 0.5f,  0.75f, 0.75f },
    //     /* Ai */ { 0.75f, 0.5f,  0.75f, 0.75f, 2.0f,  0.75f, 0.75f, 0.75f },
    //     /* Li */ { 0.75f, 0.75f, 0.75f, 0.75f, 0.75f, 0.75f, 0.0f,  3.0f  },
    //     /* Da */ { 0.75f, 0.75f, 0.75f, 0.75f, 0.75f, 0.75f, 3.0f,  0.0f  }
    // };

    private static readonly float[,] damageMultiplier = {
        //         En     Fi     Wa     Li     Ea     Ai    
        /* En */ { 1.0f,  1.0f,  1.0f,  1.0f,  1.0f,  1.0f, },
        /* Fi */ { 0.75f, 0.75f, 0.5f,  0.75f, 0.75f, 2.0f, },
        /* Wa */ { 0.75f, 2.0f,  0.75f, 0.5f,  0.75f, 0.75f,},
        /* Li */ { 0.75f, 0.75f, 2.0f,  0.75f, 0.5f,  0.75f,},
        /* Ea */ { 0.75f, 0.75f, 0.75f, 2.0f,  0.75f, 0.5f, },
        /* Ai */ { 0.75f, 0.5f,  0.75f, 0.75f, 2.0f,  0.75f,},
    };

    public static float CalculateDamage(ElementType attacker, ElementType defender, float baseDamage)
    {
        int attackerIndex = (int)attacker;
        int defenderIndex = (int)defender;

        float multiplier = damageMultiplier[attackerIndex, defenderIndex];
        return baseDamage * multiplier;
    }
}
