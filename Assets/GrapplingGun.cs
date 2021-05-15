using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingGun : MonoBehaviour
{
    private LineRenderer lr;
    public LayerMask whatIsGrappable;
    public float maxDistance = 5f;

    public Transform gunTip;
    public GameObject player;
    public float upForce = 1f;
    private SpringJoint joint;

    Camera cam;
    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = player.GetComponent<Rigidbody>();
        cam = Camera.main;
        lr = GetComponent<LineRenderer>();
    }

    Vector3 xyHit;
    Vector3 xyPos;
    Vector3 direction;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartGrapple();
        }

        if (Input.GetMouseButtonUp(0))
        {
            StopGrapple();
        }

        if (Input.GetKey(KeyCode.Q))
        {

            Debug.DrawLine(xyPos, xyHit, Color.green);
            xyPos = new Vector3(transform.position.x, transform.position.y, 0f);
            direction = xyHit - xyPos;
            rb.AddForce(upForce * direction * Time.deltaTime);
        }
    }

    void StartGrapple()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000, whatIsGrappable))
        {

            xyHit = new Vector3(hit.point.x, hit.point.y, 0f);
            xyPos = new Vector3(transform.position.x, transform.position.y, 0f);

            float distanceFromPoint = Vector3.Distance(player.transform.position, xyHit);
            if (distanceFromPoint >= maxDistance)
                return;

            joint = player.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = xyHit;

            // The distance grapple will try to keep from grappling point.
            //  joint.maxDistance = distanceFromPoint * 0.5f;
            joint.maxDistance = 15f;
            joint.minDistance = distanceFromPoint * 0.25f;

            // Change these to test it out 
            joint.spring = 30f;
            joint.damper = 1f;
            joint.massScale = 15f;

            lr.positionCount = 2;
        }
    }


    private void LateUpdate()
    {
        DrawRope();
    }

    void DrawRope()
    {
        if (!joint) return;

        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, xyHit);
    }

    void StopGrapple()
    {
        lr.positionCount = 0;
        Destroy(joint);
    }

    public bool IsGrappling()
    {
        return joint != null;
    }

    public Vector3 GetGrapplingPoint()
    {
        return xyHit;
    }
}
