using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour
{

    //
    // Geht nicht, da der AttackCollider ein Trigger ist! What to do? Raycast oder 2. non trigger Collider
    //
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("PlayerAttack"))
        {
            Debug.Log("Destructible gekillt !");
            Destroy(gameObject);
        }
    }
}
