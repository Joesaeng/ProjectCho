using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Interfaces;
using UnityEngine.AI;
using Data;
using System;

public abstract class Projectile : MonoBehaviour, IDamageDealer, IMoveable
{
    GameObject ProjectileObject { get; set; }

    public Action<IDamageDealer,Transform> OnImpact; 

    protected string _projectilePath;
    protected string _explosionPath;

    private Vector3 _destination;
    private Vector3 _direction;
    private float _attackDamage;
    private Rigidbody _rigid;
    private float _moveSpeed;
    private int _pierceCount;
    private ElementType _elementType;

    public Vector3 Destination { get => _destination; set => _destination = value; }
    public Vector3 Direction { get => _direction; set => _direction = value; }
    public float AttackDamage { get => _attackDamage; set => _attackDamage = value; }
    public Rigidbody Rigid { get => _rigid; set => _rigid = value; }
    public ElementType ElementType { get => _elementType; set => _elementType = value; }
    public float MoveSpeed { get => _moveSpeed; set => _moveSpeed = value; }
    public int PierceCount { get => _pierceCount; set => _pierceCount = value; }

    TrailRenderer TrailRenderer { get; set; }

    public virtual void Init(IData data)
    {
        Managers.CompCache.GetOrAddComponentCache(gameObject, out _rigid);
        Rigid.useGravity = false;
        Rigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ
            | RigidbodyConstraints.FreezeRotationY;
        ProjectileData projectileData = data as ProjectileData;
        _projectilePath = "Effects/Projectiles/" + projectileData.projectileName;
        _explosionPath = "Effects/Explosions/" + projectileData.explosionName;

        ProjectileObject = Managers.Resource.Instantiate(_projectilePath, transform);
        ProjectileObject.transform.localScale = Vector3.one;

        TrailRenderer = null;
        TrailRenderer = ProjectileObject.GetComponentInChildren<TrailRenderer>();

        StartCoroutine(CoMoveUpdate());
    }

    public abstract void InitDamageDealer(IData data);

    public virtual void InitMoveable(IData data)
    {
        ProjectileData projectileData = data as ProjectileData;
        MoveSpeed = projectileData.baseMoveSpeed;
    }

    public void SetDir(Vector3 dir)
    {
        Direction = dir;
        if(dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    public void Move()
    {
        Rigid.velocity = Direction * MoveSpeed;
    }
    IEnumerator CoMoveUpdate()
    {
        while (true)
        {
            yield return null;
            Move();
        }
    }
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Wall") || other.gameObject.CompareTag("FrontWall"))
        {
            ShowHitEffect();
            DestroyBullet();
            return;
        }
        if (other.gameObject.TryGetComponent(out IHitable hitable))
        {
            hitable.TakeDamage(this);
            ShowHitEffect();

            OnImpact?.Invoke(this,transform);
            
            PierceCount--;

            if (PierceCount == 0)
            {
                DestroyBullet();
            }
        }
    }

    protected void ShowHitEffect()
    {
        GameObject obj = Managers.Resource.Instantiate(_explosionPath, transform.position);
        Managers.CompCache.GetOrAddComponentCache(obj, out HitEffect hitEffect);
        hitEffect.Init();
    }

    protected void DestroyBullet()
    {
        OnImpact = null;
        if (TrailRenderer != null)
        {
            TrailRenderer.Clear();
        }
        Managers.Resource.Destroy(ProjectileObject);
        Managers.Resource.Destroy(gameObject);
    }
}
