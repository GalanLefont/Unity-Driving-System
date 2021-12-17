using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringHelp : MonoBehaviour
{
    [Range(0,1)]
    public float steeringHelpInfluence;

    Rigidbody chasis;

    public float optimalMaxSteeringAngle;
    public float maxHelpAngle;

    [HideInInspector]
    public float steeringMaxAngle;
    [HideInInspector]
    public float steeringZeroAngle;

    // Start is called before the first frame update
    void Start()
    {
        chasis = GetComponentInParent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        Vector3 relativeRoadVelocity = transform.InverseTransformVector(chasis.GetPointVelocity(transform.position));
        relativeRoadVelocity -= relativeRoadVelocity.y * Vector3.up;

        float angle = Vector3.Angle(Vector3.right, relativeRoadVelocity);
        int dir = relativeRoadVelocity.z > 0 ? 1 : -1;

        if (angle > maxHelpAngle)
        {
            steeringZeroAngle = 0;
            steeringMaxAngle = 90;
            return;
        }
        steeringMaxAngle = Mathf.Max(Mathf.Abs(angle + optimalMaxSteeringAngle), Mathf.Abs(angle - optimalMaxSteeringAngle));

        Vector3 chasisToRoadRelativeVelocity = transform.InverseTransformVector(chasis.velocity);
        chasisToRoadRelativeVelocity -= chasisToRoadRelativeVelocity.y * Vector3.up;
        dir = chasisToRoadRelativeVelocity.z > 0 ? 1 : -1;
        angle = Vector3.Angle(Vector3.right, chasisToRoadRelativeVelocity) * steeringHelpInfluence;
        steeringZeroAngle = angle * -dir;
        
    }
}
