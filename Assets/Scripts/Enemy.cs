using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Collider))]

public class Enemy : MonoBehaviour
{
    public int id;
    public string title;
    public int health;
    public int damage;
    public float moveSpeed;
    public float distanceX;
    public float distanceY;
    public float vectorDistance;
    public float attackRange;
    public float moveRange;
    public float attackDuration;
    public bool isAttacking;
    public float dieDuration;
    public bool isDead = false;

    public AudioClip attackSound;
    public AudioClip dieSound;

    public Vector3 shrinkLimit;
    public float shrinkSpeed;

 

    public GameObject dieVFX;
    public GameObject attackVFX;
    public Collider attackCol;
    public Projectile projectile;
    public GameObject player;
    public CharacterController characterController;
    NavMeshAgent agent;

    #region Movements towards Player
    public virtual void MoveToPlayerX()
    {
        if (distanceX <= moveRange)
        {
            agent.isStopped = false;
            agent.SetDestination(player.transform.position);
            LookAtTarget(player.transform);
        }
        else if (distanceX <= agent.stoppingDistance || distanceX >= moveRange)
        {
            agent.isStopped = true;
            transform.rotation = Quaternion.identity;
        }
    }

    public virtual void MoveToPlayerY()
    {
        if (distanceY <= moveRange)
        {
            agent.isStopped = false;
            agent.SetDestination(player.transform.position);
        }
        else
            agent.isStopped = true;
    }

    public virtual void MoveToPlayerVectorDistance()
    {
        if (vectorDistance <= moveRange)
        {
            agent.isStopped = false;
            agent.SetDestination(player.transform.position);
        }
        else
            agent.isStopped = true;
    }
    #endregion

    #region Calculate Distances To Player
    public virtual void CalculateDistanceToPlayerX()
    {
        distanceX = Mathf.Abs(transform.position.x - player.transform.position.x);
    }
    public virtual void CalculateDistanceToPlayerY()
    {
        distanceY = Mathf.Abs(transform.position.y - player.transform.position.y);
    }
    public virtual void CalculateDistanceToPlayerVector()
    {
        distanceX = Mathf.Abs(transform.position.x - player.transform.position.x);
        distanceY = Mathf.Abs(transform.position.y - player.transform.position.y);
        vectorDistance = new Vector3(distanceX, distanceY, 0f).sqrMagnitude;
    }
    #endregion

    public virtual void GetPlayer()
    {
        player = GameManager.instance.player;
        characterController = player.GetComponent<CharacterController>();
    }

    public virtual void LookAtTarget(Transform target)
    {
        transform.LookAt(target);
    }

    public virtual IEnumerator DieCR()
    {
        isDead = true;
        Instantiate(dieVFX, transform);
        yield return new WaitForSeconds(dieDuration);

        Destroy(gameObject);
    }

    public virtual void LerpSize(Vector3 size, float lerpTime)
    {
        transform.localScale = Vector3.Lerp(transform.localScale, size, lerpTime * Time.deltaTime);
    }

    public virtual IEnumerator AttackCR()
    {
        isAttacking = true;
        GameObject vfxInstance = Instantiate(attackVFX, transform);
        attackCol.enabled = true;

        yield return new WaitForSeconds(attackDuration);

        Destroy(vfxInstance);
        attackCol.enabled = false;
        isAttacking = false;
    }

    public virtual void InitializeNavMesh()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
    }

    public virtual void TakeDamage(int amount)
    {
        health -= amount;
    }

    public virtual void Start()
    {
        GetPlayer();
    }
}
