using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookController : MonoBehaviour
{
    public float hookRange = 10000000f;
    public float speed;

    public void PlayerToMousePos()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1f));
        //  Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f);
        Vector3 playerPos = new Vector3(transform.position.x, transform.position.y, 0f);
        Vector3 direction = mousePos - transform.position;

     //   Debug.DrawLine(transform.position, mousePos, Color.red);

        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, hookRange))
        {
            Debug.Log(hit.collider.name);
            rb.AddForce(direction * speed, ForceMode.VelocityChange);
        }
    }
    Vector3 direction;
    RaycastHit hitInfo;

    LineRenderer lineRend;
    Vector3[] linePositions = new Vector3[2];
    Vector3 xyHit;
    Vector3 xyPos;
    ConfigurableJoint myJoint;

    public void MouseToWorld()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hitInfo, 1000))
        {
            lineRend.enabled = true;

            xyHit = new Vector3(hitInfo.point.x, hitInfo.point.y, 0f);
            xyPos = new Vector3(transform.position.x, transform.position.y, 0f);

            direction = xyHit - xyPos;
                  
            Debug.Log(hitInfo.point);
            Debug.DrawLine(xyPos, xyHit);

            if (Input.GetMouseButton(0))
            {
                myJoint.connectedAnchor = hitInfo.point;
               // rb.AddForce(direction * speed * Time.deltaTime, ForceMode.VelocityChange);
            }
            else
            {
 
                lineRend.enabled = false;
            }

        }
        else
        {
            lineRend.enabled = false;
        }
    }

    Rigidbody rb;
    Vector3 origin;

    // Start is called before the first frame update
    void Start()
    {
        myJoint = GetComponent<ConfigurableJoint>();
        lineRend = GetComponent<LineRenderer>();
        origin = transform.position;
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.O))
        //{
        //    transform.position = origin;
        //}
        if (Input.GetKey(KeyCode.P))
        {
            PlayerToMousePos();
        }

        linePositions[0] = xyPos;
        linePositions[1] = xyHit;
        lineRend.SetPositions(linePositions);
        MouseToWorld();
    }
}
