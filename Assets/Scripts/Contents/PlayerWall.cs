using Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWall : MonoBehaviour, IHitable
{
    private float _maxHp;
    private float _curHp;
    private bool _isDead;

    public float MaxHp { get => _maxHp; set => _maxHp = value; }
    public float CurHp { get => _curHp; set => _curHp = value; }
    public bool IsDead { get => _isDead; set => _isDead = value; }
    public Transform Tf { get => transform; }

    public void InitHitable(IData data)
    {
        MaxHp = 3000;
        CurHp = MaxHp;
    }

    public void TakeDamage(IDamageDealer dealer)
    {
        CurHp -= dealer.AttackDamage;
        if(CurHp < 0)
        {
            // 게임오버
        }
    }
}
