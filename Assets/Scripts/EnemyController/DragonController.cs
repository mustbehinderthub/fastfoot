using System.Collections;
using UnityEngine;

public class DragonController : Enemy
{
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        InitializeNavMesh();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isDead)
        {
            if (player != null)
            {
                CalculateDistanceToPlayerX();
                MoveToPlayerX();
            }

            if (distanceX <= attackRange && !isAttacking)
                Attack();
        }
        if (isDead)
        {
            if (transform.localScale.x >= shrinkLimit.x)
                LerpSize(shrinkLimit, shrinkSpeed);
        }
    }

    public void Attack()
    {
        StartCoroutine(AttackCR());
    }

    public override IEnumerator AttackCR()
    {
        return base.AttackCR();
    }

    public override IEnumerator DieCR()
    {
        return base.DieCR();
    }

    private void OnTriggerEnter(Collider col)
    {
        if (!isDead)
        {
            if (col.CompareTag("PlayerAttack"))
            {
                TakeDamage(characterController.damage);
                DamageTextController.CreateDamageText(characterController.damage.ToString(), transform, DamageTextController.damageTextPlayer);
                Debug.Log("Enemy took damage from player!");
            }
        }
    }
}
