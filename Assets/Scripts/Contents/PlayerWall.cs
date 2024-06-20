using Define;
using Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallData : ISetData
{
    int IData.Id => id;
    public int id;
    public float maxHp;

}

public class PlayerWall : MonoBehaviour, IHitable
{
    private float _maxHp;
    private float _curHp;
    private bool _isDead;
    private Transform Wall { get; set; }
    public Action<int> OnUpdatePlayerHp;
    public Action<GameoverType> OnGameOver;

    public float MaxHp { get => _maxHp; set => _maxHp = value; }
    public float CurHp { get => _curHp; set => _curHp = value; }
    public bool IsDead { get => _isDead; set => _isDead = value; }
    public Transform Tf { get => transform; }

    public void InitHitable(IData data)
    {
        MaxHp = 3000;
        CurHp = MaxHp;
        IsDead = false;
        Wall = GameObject.Find("LifeWall").transform;
    }

    public bool TakeDamage(IDamageDealer dealer)
    {
        if (IsDead)
            return true;

        CurHp -= dealer.AttackDamage;
        float scaleY = CurHp > 0 ? (CurHp / MaxHp) : 0;
        Wall.localScale = new Vector3(Wall.localScale.x, scaleY, Wall.localScale.z);
        if (CurHp < 0)
        {
            // 게임오버
            CurHp = 0;
            IsDead = true;
            OnUpdatePlayerHp?.Invoke(Mathf.FloorToInt(CurHp));
            OnGameOver?.Invoke(GameoverType.Gameover);
            OnGameOver = null;
            OnUpdatePlayerHp = null;
            return true;
        }
        OnUpdatePlayerHp?.Invoke(Mathf.FloorToInt(CurHp));
        return false;
    }
}
