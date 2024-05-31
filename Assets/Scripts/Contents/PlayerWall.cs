using Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallData : ISetData
{
    int IData.id => id;
    public int id;
    public float maxHp;

}

public class PlayerWall : MonoBehaviour, IHitable
{
    private float _maxHp;
    private float _curHp;
    private bool _isDead;
    private Transform Wall { get; set; }

    public float MaxHp { get => _maxHp; set => _maxHp = value; }
    public float CurHp { get => _curHp; set => _curHp = value; }
    public bool IsDead { get => _isDead; set => _isDead = value; }
    public Transform Tf { get => transform; }

    public void InitHitable(IData data)
    {
        MaxHp = 3000;
        CurHp = MaxHp;
        Wall = GameObject.Find("ArcaneWall").transform;
    }

    public void TakeDamage(IDamageDealer dealer)
    {
        CurHp -= dealer.AttackDamage;
        float scaleY = CurHp > 0 ? 2 * (CurHp / MaxHp) : 0;
        Wall.localScale = new Vector3(Wall.localScale.x, scaleY, Wall.localScale.z);
        if (CurHp < 0)
        {
            // 게임오버
            Debug.Log("GameOver");
        }
    }
}
