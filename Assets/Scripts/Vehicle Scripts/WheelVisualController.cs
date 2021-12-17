using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelVisualController : MonoBehaviour
{
    SuspensionAndWheel controllerWheel;
    public Transform meshController;
    public Transform particleContainer;
    
    void Start()
    {
        controllerWheel = GetComponentInParent<SuspensionAndWheel>();     
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 newEuler = meshController.localEulerAngles - Vector3.forward * controllerWheel.wheelVisualAngularVelocity * Mathf.Rad2Deg * Time.deltaTime;

        transform.position = transform.parent.position - transform.parent.up * controllerWheel.wheelPositionCurrent;
        particleContainer.position = transform.parent.position - transform.parent.up * (controllerWheel.wheelPositionCurrent + controllerWheel.wheelRadius);
        meshController.localRotation = Quaternion.Euler(0, 0, newEuler.z);        

       // Debug.DrawLine(meshController.position, meshController.position + meshController.right * controllerWheel.wheelRadius, Color.blue);
    }

    private void FixedUpdate()
    {
        transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, controllerWheel.wheelSteeringAngle, transform.localRotation.eulerAngles.z);
    }
}
