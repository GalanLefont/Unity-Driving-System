using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public enum cameraBehaviours { NEW, OLD};
    public cameraBehaviours behaviours;

    public Rigidbody vehicleRigidbody;
    public Transform target;
    public Transform targetPostion;

    public float distance;
    public float height;

    [Range(0, 1)]
    public float smoothing;
    public float minVelocity;
    Vector3 orientation;

    public float wait = 1f;
    // Start is called before the first frame update
    void Start()
    {
        orientation = target.right;
    }


    void FixedUpdate()
    {
        if (behaviours == cameraBehaviours.OLD)
        {
            transform.LookAt(target);
            transform.position += (targetPostion.position - transform.position) * 0.1F;
        }
        else
        {            
            if(vehicleRigidbody.velocity.magnitude > minVelocity && wait < 0)                
                orientation = vehicleRigidbody.velocity.normalized;
            else
                wait -= Time.deltaTime;

            Vector3 targetPosition = target.position + orientation * distance + Vector3.up * height;

            transform.position += (targetPosition - transform.position) * smoothing;
            transform.LookAt(target);
        }
    }
}
