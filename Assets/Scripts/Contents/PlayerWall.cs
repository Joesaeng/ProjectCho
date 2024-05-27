using Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWall : MonoBehaviour, IHitable
{
    private float _maxHp;
    private float _curHp;

    public float MaxHp { get => _maxHp; set => _maxHp = value; }
    public float CurHp { get => _curHp; set => _curHp = value; }

    public void InitHitable(IData data)
    {
        MaxHp = 3000;
        CurHp = MaxHp;
    }

    public void TakeDamage(IDamageDealer dealer)
    {
        CurHp -= dealer.AttackDamage;
        string type = dealer.GetType().Name;
        if(CurHp < 0)
        {
            // 게임오버
        }
    }
}
