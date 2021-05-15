using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;
    Camera cam;
    public float lerpTime;
    public float yOffset;

    public Vector3 offset;
    public float distance;

    Vector3 desiredPos;
    public Vector3 debug; 

    void FollowCharacter()
    {
      //  desiredPos = new Vector3(player.transform.position.x, yOffset, transform.position.z);
        transform.position = desiredPos;
    }

    void CameraMoveX()
    {
        if (player.transform.position.x >= cam.ViewportToWorldPoint(new Vector3(1f, 1f, transform.position.z)).x)
        {            
            desiredPos = Vector3.Lerp(transform.position, new Vector3(player.transform.position.x , transform.position.y, transform.position.z),lerpTime);           
            FollowCharacter();
        }
        if (player.transform.position.x <= cam.ViewportToWorldPoint(new Vector3(0f, 0f, transform.position.z)).x)
        {
            desiredPos = Vector3.Lerp(transform.position, new Vector3(player.transform.position.x, transform.position.y, transform.position.z), lerpTime);
            FollowCharacter();
        }  
    }

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();       
    }

    // Update is called once per frame
    void Update()
    {
        FollowCharacterInstant();  
    }

    void FollowCharacterInstant()
    {
        transform.position = Vector3.Lerp(transform.position, new Vector3(player.transform.position.x, player.transform.position.y + 4f,transform.position.z), lerpTime);
    }
}
