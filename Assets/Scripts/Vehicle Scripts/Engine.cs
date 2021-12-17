using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine : MonoBehaviour
{
    [Header("Vehicle")]
    public Transform centerOfMass;
    VehicleController controller;
    Rigidbody chasis;
    Queue<SuspensionAndWheel> wheels = new Queue<SuspensionAndWheel>();

    [Header("Engine")]
    public float engineIntertia = 1; 
           float engineAngularVelocity;
    public float engineClutchedTorque;
    public float engineClutchedDrag;          

    public AnimationCurve engineTorqueVsRPM;
    public float engineTorqueScale;
           float RPM { get { return radToRPM(engineAngularVelocity); } }

    [Header("Diferencial")]
    public differentialType differential;
    public enum differentialType { MAL, LSD, LOOCKED}

    [Header("LSD")]
    public float LsdMaxSlip;
    public float LsdMaxDistribution;

    [Header("Transmisión")]
    [Range(-4, 4)]
    public float[] gearRatios;
    public float gearRatio { get { return gearRatios[currentGear] * finalDriveRatio; } }
    public float finalDriveRatio;
    int currentGear;
    public bool clutch;
    public bool launch;

    public float minOptimalRPM;
    public float maxOptimalRPM;

    public float revLimiterRPM;

    public float reverseMinimunSpeed;
    public float reverseWait;
    float reverseWaiting;

    public bool handbrakeClutch = true; 

    [Header("Tachometer")]
    public Tachometer tacho;
    public float rpmOutputSmoothingFactor;
    [Header("Sound")]
    public AudioClip engineSoundClip;
    public float engineSoundClipBaseRPM;
    public EngineSoundController engineSoundController;

    void Start()
    {
        controller = GetComponent<VehicleController>();
        chasis = GetComponent<Rigidbody>();

        initializeSound();
    }

    // Update is called once per frame
    void Update()
    {
        if(handbrakeClutch)
            clutch = controller.controls.handBrake || controller.controls.clutch;
        else
            clutch = controller.controls.clutch;

        updateTachometer();
        updateEngineSound();

        Vector3 relativeVelocity = transform.InverseTransformVector(chasis.velocity);
        relativeVelocity = new Vector3(relativeVelocity.x, 0, 0);     
        
        Debug.DrawLine(transform.position, 
            transform.position + transform.TransformVector(relativeVelocity), Color.green, 0, false);
    }

    private void FixedUpdate()
    {
        if (float.IsNaN(engineAngularVelocity) || float.IsInfinity(engineAngularVelocity))
        {
            engineAngularVelocity = 0;
        }

        runAutomaticGearShift();
        runDifferential();

        chasis.centerOfMass = centerOfMass.localPosition;
    }

    float transmisionTorque(float angularVelocity)
    {        
        float torque = 0;

        if(launch && !clutch)
        {
            launch = false;

            float angularVelocityDiference = engineAngularVelocity - Mathf.Clamp(angularVelocity * gearRatio, 0, revLimiterRPM * 1.4f);

            torque += angularVelocityDiference * engineIntertia;
        }

        if (!clutch)
            engineAngularVelocity = angularVelocity * gearRatio;


        torque += engineTorqueVsRPM.Evaluate(radToRPM(engineAngularVelocity) / 1000) * engineTorqueScale * controller.controls.throttle;


        if(UICarDebbuger.main != null)
            UICarDebbuger.main.updateDebug(torque, RPM, gearRatio);

        if (!clutch)
            return torque * gearRatio;

        launch = true;
        engineAngularVelocity += torque * engineClutchedTorque / engineIntertia * Time.deltaTime;
        if(RPM > 1000)
            engineAngularVelocity -= engineClutchedDrag / engineIntertia * Time.deltaTime;
        else
            engineAngularVelocity += torque * engineClutchedTorque / engineIntertia * Time.deltaTime;
        return 0;
    }

    void runDifferential()
    {
        switch(differential)
        {
            case differentialType.LOOCKED:
                differentialLocked();
                break;
            case differentialType.LSD:
                differentialLsd();
                break;
            case differentialType.MAL:
                differentialMal();
                break;
        }
    }

    void differentialLocked()
    {
        float averageWheelSpeed = 0;
        float totalAngularMass = 0;
        float averageWheelRadius = 0;

        foreach(SuspensionAndWheel wheel in wheels)
        {            
            totalAngularMass += wheel.wheelAngularMass;
            averageWheelRadius += wheel.wheelRadius;
            averageWheelSpeed += wheel.wheelAngularVelocity;
        }
        averageWheelRadius /= wheels.Count;
        averageWheelSpeed /= wheels.Count;

        float differentialAngularVelocity = averageWheelSpeed;
        differentialAngularVelocity += transmisionTorque(differentialAngularVelocity) / (totalAngularMass * Mathf.Pow(averageWheelRadius, 2) / 2) * Time.deltaTime;

        float appliedTorque = 0;
        foreach (SuspensionAndWheel wheel in wheels)
        {
            wheel.wheelEngineTorque = (-wheel.wheelAngularVelocity + differentialAngularVelocity) * wheel.wheelAngularInertia / Time.deltaTime;
            appliedTorque += (-wheel.wheelAngularVelocity + differentialAngularVelocity) * wheel.wheelAngularInertia / Time.deltaTime;
        }
    }

    void differentialLsd()
    {
        float averageAngularVelocity = 0;
        
        foreach (SuspensionAndWheel wheel in wheels)       
            averageAngularVelocity += wheel.wheelAngularVelocity;
        
        averageAngularVelocity /= wheels.Count;

        float torque = transmisionTorque(averageAngularVelocity) / wheels.Count;

        float appliedTorque = 0;
        foreach (SuspensionAndWheel wheel in wheels)
        {
            float slip = (wheel.wheelAngularVelocity / averageAngularVelocity - 1);

            wheel.wheelEngineTorque = (1 - Mathf.Clamp(slip / LsdMaxSlip, -1, 1) * LsdMaxDistribution) * torque;
            appliedTorque += (1 - Mathf.Clamp(slip / LsdMaxSlip, -1, 1) * LsdMaxDistribution) * torque;
        }
    }

    void differentialMal()
    {
        float averageAngularVelocity = 0;

        foreach(SuspensionAndWheel wheel in wheels)
        {
            averageAngularVelocity += wheel.wheelAngularVelocity;
        }

        averageAngularVelocity /= wheels.Count;

        float torque = transmisionTorque(averageAngularVelocity);

        foreach(SuspensionAndWheel wheel in wheels)        
            wheel.wheelEngineTorque = torque /wheels.Count;            
       
    }

    void runAutomaticGearShift()
    {
        if (transform.TransformVector(chasis.velocity).x * controller.dir < reverseMinimunSpeed && controller.controls.brakes > 0.7f) //Shift To Reverse
        {
            reverseWaiting -= Time.deltaTime;

            if (reverseWaiting <= 0)
            {
                reverseWaiting = reverseWait;
                controller.reverse = !controller.reverse;
            }
        }
        else
            reverseWaiting = reverseWait;
            
        if (controller.reverse)
        {
            currentGear = 0;
            return;
        }
        else if (currentGear == 0) currentGear = 1;
     

        if (!clutch && !launch)
        {
            if (RPM > maxOptimalRPM)
                shiftGear(1);
            else if (RPM < minOptimalRPM)
                shiftGear(-1);
        }
    }

    void shiftGear(int a)
    {        
        if(currentGear + a > 0 && currentGear + a < gearRatios.Length)
        {
            currentGear = currentGear + a;
        }
    }
    

    float radToRPM(float rads)
    {
        return rads * Mathf.Rad2Deg / 360 * 60;
    }

    void updateTachometer()
    {
        if (tacho != null)
            tacho.setTacho(RPM, revLimiterRPM, currentGear, chasis.velocity.magnitude, rpmOutputSmoothingFactor);
     
    }
    
    void updateEngineSound()
    {
        if (float.IsNaN(engineAngularVelocity) || float.IsInfinity(engineAngularVelocity))        
            engineAngularVelocity = 0;        

        engineSoundController.setSound(RPM, controller.controls.throttle);
    }

    void initializeSound()
    {
        engineSoundController.initializeSound(engineSoundClip, engineSoundClipBaseRPM, revLimiterRPM, rpmOutputSmoothingFactor);
    }

    public void connectWheel(SuspensionAndWheel wheel)
    {
        wheels.Enqueue(wheel);
    }
}


