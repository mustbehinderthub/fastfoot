using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeSize : MonoBehaviour
{
    public float growTime;
    public float shrinkTime;
    public float maxSize;
    public bool isHit;
    public Vector3 hit;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!isHit)
            Grow();
        if (isHit)
            Shrink();
    }

    public void Grow()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(maxSize, transform.localScale.y, 25f), Time.deltaTime * growTime);
    }

    public void Shrink()
    {
        float distanceShrink = GetComponentInParent<Rigidbody>().velocity.magnitude * (GetComponentInParent<CharacterController>().transform.position - hit).magnitude * Time.deltaTime;
        transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(0f, transform.localScale.y, 0f), Time.deltaTime * distanceShrink * shrinkTime);
        if (Vector3.Distance(GetComponentInParent<CharacterController>().transform.position, hit) <= 5f)
        {
            Destroy(gameObject);
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        hit = collision.GetContact(0).point;
       // hit = collision.transform.position;
        GetComponentInParent<CharacterController>().isHookHit = true;
        GetComponentInParent<CharacterController>().isMovementLocked = false;
        isHit = true;
        Debug.Log("Hook collided!");
    }
}
