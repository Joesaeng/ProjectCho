using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Interfaces;
using UnityEngine.AI;
using Data;

[RequireComponent(typeof(Rigidbody))]
public abstract class Projectile : MonoBehaviour, IDamageDealer, IMoveable
{
    GameObject ProjectileObject { get; set; }

    private string _projectilePath;
    private string _explosionPath;

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

    public virtual void Init(IData data)
    {
        Managers.CompCache.GetOrAddComponentCache(gameObject, out _rigid);
        Rigid.useGravity = false;
        Rigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ
            | RigidbodyConstraints.FreezeRotationY;
        ProjectileData projectileData = data as ProjectileData;
        PierceCount = projectileData.pierceCount;
        _projectilePath = "Projectiles/" + projectileData.projectileName;
        _explosionPath = "Explosions/" + projectileData.explosionName;

        if(ProjectileObject == null)
        {
            ProjectileObject = Managers.Resource.Instantiate(_projectilePath, transform);
        }
        else
        {
            if (ProjectileObject.name != projectileData.projectileName)
            {
                Managers.Resource.Destroy(ProjectileObject);
                ProjectileObject = Managers.Resource.Instantiate(_projectilePath, transform);
            }
            else
            {
                ProjectileObject.SetActive(true);
                ProjectileObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }
        }

        StartCoroutine(CoMoveUpdate());
    }

    public abstract void InitDamageDealer(IData data);

    public abstract void InitMoveable(IData data);

    public void SetDir(Vector3 targetPos)
    {
        Destination = targetPos;
        Direction = (Destination - transform.position).normalized;
        transform.LookAt(Direction);
    }

    public void Move()
    {
        Rigid.velocity = Direction * MoveSpeed;
    }
    IEnumerator CoMoveUpdate()
    {
        while(true)
        {
            yield return null;
            Move();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Wall"))
        {
            Managers.Resource.Destroy(gameObject);
            return;
        }
        if (collision.gameObject.TryGetComponent<IHitable>(out IHitable hitable))
        {
            hitable.TakeDamage(this);
            GameObject obj = Managers.Resource.Instantiate(_explosionPath, transform.position);
            Managers.CompCache.GetOrAddComponentCache(obj, out ProjectileExplosion projectileExplosion);
            projectileExplosion.Init();

            PierceCount--;

            if(PierceCount == 0)
            {
                StopCoroutine(CoMoveUpdate());
                Managers.Resource.Destroy(gameObject);
            }
        }
    }
}
