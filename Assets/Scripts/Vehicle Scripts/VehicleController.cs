using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleController : MonoBehaviour
{
    [HideInInspector]
    public float div = 5;
    [HideInInspector]
    public bool stop;
    public struct Controls
    {
        public float throttle;
        public float brakes;
        public float steering;
        public bool clutch;
        public bool handBrake;
    }

    public Controls controls;
    public bool reverse;

    public int dir { get { return !reverse ? 1 : -1; } }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        controls.throttle = Mathf.Clamp(Input.GetAxis("Vertical") * dir, 0,1);
        controls.brakes = -Mathf.Clamp(Input.GetAxis("Vertical") * dir, -1,0);
        controls.steering = Input.GetAxis("Horizontal");
        controls.handBrake = Input.GetButton("Jump");
        controls.clutch = Input.GetButton("Fire1");

        if (stop)
        {
            controls.brakes = 1;
            controls.throttle = 0;
            controls.steering = 0;
        }
        //controls.throttle = Mathf.Clamp(Input.GetAxisRaw("Throttle") * dir, 0, 1);
      //  controls.brakes = -Mathf.Clamp(Input.GetAxisRaw("Throttle") * dir, -1, 0);

    }
}
