using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Interfaces;
using UnityEngine.AI;
using Data;

public abstract class Projectile : MonoBehaviour, IDamageDealer, IMoveable
{
    GameObject ProjectileObject { get; set; }

    protected string _projectilePath;
    protected string _explosionPath;

    private Vector3 _destination;
    private Vector3 _direction;
    private float _attackDamage;
    private Rigidbody _rigid;
    private float _moveSpeed;
    private int _pierceCount;

    public Vector3 Destination { get => _destination; set => _destination = value; }
    public Vector3 Direction { get => _direction; set => _direction = value; }
    public float AttackDamage { get => _attackDamage; set => _attackDamage = value; }
    public Rigidbody Rigid { get => _rigid; set => _rigid = value; }
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
        _projectilePath = "ParticleEffects/Projectiles/" + projectileData.projectileName;
        _explosionPath = "ParticleEffects/Explosions/" + projectileData.explosionName;

        ProjectileObject = Managers.Resource.Instantiate(_projectilePath, transform);

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
        if (other.gameObject.CompareTag("Wall"))
        {
            Managers.Resource.Instantiate(_explosionPath, transform.position);
            DestroyBullet();
            return;
        }
        if (other.gameObject.TryGetComponent(out IHitable hitable))
        {
            hitable.TakeDamage(this);
            GameObject obj = Managers.Resource.Instantiate(_explosionPath, transform.position);
            Managers.CompCache.GetOrAddComponentCache(obj, out ProjectileExplosion projectileExplosion);
            projectileExplosion.Init();

            PierceCount--;

            if (PierceCount == 0)
            {
                if (TrailRenderer != null)
                {
                    TrailRenderer.Clear();
                }
                DestroyBullet();
            }
        }
    }

    protected void DestroyBullet()
    {
        Managers.Resource.Destroy(ProjectileObject);
        Managers.Resource.Destroy(gameObject);
    }
}
