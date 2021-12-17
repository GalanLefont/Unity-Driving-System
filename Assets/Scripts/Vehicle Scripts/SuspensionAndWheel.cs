using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class SuspensionAndWheel : MonoBehaviour
{
    [Header("Vehicle")]
    public Rigidbody chasis;
    public WheelVisualController wheelVisual;
    public VehicleController controller;
    public Engine engine;

    [Header("Suspensión")]
           float vLong;
    public float suspensionSpringRate;
    public float suspensionDampeningRate;

           float wheelVerticalSpeed;
    public float wheelRadius;
    public float wheelPositionInitial;
    [HideInInspector]
    public float wheelPositionCurrent;
    public float wheelPositionMax;
    public float wheelMass;

    [Header("Rueda")]
    [HideInInspector]
    public float wheelLoad;
    [HideInInspector]
    public float wheelSlipRatio;
           float wheelSlipAngle;
    [HideInInspector]
    public float wheelAngularVelocity;
    public float wheelAngularMass;
    [HideInInspector]
    public float wheelVisualAngularVelocity;
    public float wheelAngularInertia { get { return wheelAngularMass * Mathf.Pow(wheelRadius, 2) / 2; } }

    public AnimationCurve[] slipRatioCurves;
    public float[] slipRatioCurveAngleValues;
    public float slipRatioCurveScale;

    public AnimationCurve[] slipAngleCurves;
    public float[] slipAngleCurveSlipValues;
    public float slipAngleCurveScale;

    [HideInInspector]
    public float wheelEngineTorque;

    public float wheelRollingResistance;

    [Header("Frenos")]
    public float brakeForce;
    public bool handBrake;
    public float handBrakeForce;
    

    public float absMinVelocity;
    public float absTargetSlip;

    [Header("Direccion")]
    public float wheelMaxSteering;
    public float wheelSteeringSpeed;
    [HideInInspector]
    public float wheelSteeringAngle;

    public SteeringHelp steeringHelper;

    [Header("Superficie")]
    Vector3 surfaceNormal;
    int currentSurface;

    public float[] surfacesGrips;
    public string[] surfacesTags;
    public GameObject[] surfacesParticleControllers;

    ParticleSystem[] surfacesInstantiatedParticleSystem;

    public float[] surfaceEmissionMinSlip;
    public float[] surfaceEmissionMaxSlip;
    public int[] surfaceEmissionMax;

    public float minEmitWheelLoad;

    float surfaceGrip { 
        get 
        {
            if (surfacesGrips.Length > 0)
                return surfacesGrips[currentSurface];
            return 1;
        } }

    [Header("Debug")]
    public float susLoad;

    [Header("Explicacion")]
    public bool explicar;
    public TextMeshProUGUI resultado;
    public TextMeshProUGUI denominador;
    public TextMeshProUGUI numerador;
    public bool explicarSlipRatio;
    public TextMeshProUGUI[] formulaEnPantalla = new TextMeshProUGUI[7];

    public bool limitar;
    public bool explicarLimitar;
    public TextMeshProUGUI[] formulaDeExplicarEnPantalla;

    // Start is called before the first frame update
    void Start()
    {
        chasis = GetComponentInParent<Rigidbody>();
        wheelVisual = GetComponentInChildren<WheelVisualController>();
        controller = GetComponentInParent<VehicleController>();

        initializeParticles();

        if (engine != null) engine.connectWheel(this);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        updateSuspension();
        runSteering();

        longitudinalCalculations();
        lateralCalculations();

        runParticles();
    }


    void updateSuspension()
    {
        float forceInSuspension = calculateHookesLaw(wheelPositionInitial - wheelPositionCurrent, suspensionSpringRate);

        //Acelerar velocidad vertical de la rueda
        wheelVerticalSpeed -= ((forceInSuspension + wheelMass * -transform.InverseTransformVector(Physics.gravity).y) / wheelMass) * Time.deltaTime;
        forceInSuspension = 0;

        //Actualizar posicion de la rueda
        wheelPositionCurrent -= wheelVerticalSpeed * Time.deltaTime;

        bool grounded = false;

        float distanceToGround = calculateDistanceToGround();
        float distanceUnderground = wheelPositionCurrent + wheelRadius - distanceToGround;
        
        if(distanceUnderground > 0 && wheelPositionCurrent < wheelPositionMax)
        {
            grounded = true;
            //Amortiguacion del chasis
            chasis.AddForceAtPosition(transform.InverseTransformVector
                (chasis.GetPointVelocity(transform.position)).y * suspensionDampeningRate * -transform.up, transform.position, ForceMode.Acceleration);

            //Debug.DrawLine(transform.position, transform.position + (transform.InverseTransformVector
           //     (chasis.GetPointVelocity(transform.position)).y * suspensionDampeningRate * -transform.up) * 2, Color.red, Time.deltaTime, false);
            forceInSuspension = ((distanceUnderground / Time.deltaTime * 2) / Time.deltaTime) * wheelMass;           
            

            wheelPositionCurrent = distanceToGround - wheelRadius;
            wheelVerticalSpeed = 0;            
        }

        
        wheelVerticalSpeed -= wheelVerticalSpeed * suspensionDampeningRate;        
        wheelLoad = forceInSuspension;

    

        chasis.AddForceAtPosition(surfaceNormal * forceInSuspension, transform.position);


        //Debug.DrawLine(transform.position, transform.position + -transform.up * (wheelPositionCurrent + wheelRadius), grounded ? Color.red : Color.white);
    }

    void lateralCalculations()
    {
        Vector3 relativeRoadVelocity = wheelVisual.transform.InverseTransformVector(
            chasis.GetPointVelocity(transform.position - transform.up * 
            (wheelPositionCurrent + wheelRadius)));

        relativeRoadVelocity -= relativeRoadVelocity.y * Vector3.up;        

        wheelSlipAngle = Vector3.Angle(
            Vector3.ProjectOnPlane(Vector3.right, surfaceNormal), 
            Vector3.ProjectOnPlane(relativeRoadVelocity, surfaceNormal));
        
        if(wheelSlipAngle >= 90)
        {
            wheelSlipAngle = 180 - wheelSlipAngle;
        }

        int dir = relativeRoadVelocity.z < 0 ? -1 : 1;
        float force = calculateSlipAngleCurveValues() * slipAngleCurveScale 
            * wheelLoad * surfaceGrip;

        force = Mathf.Clamp(force, 0, Mathf.Abs(relativeRoadVelocity.z) 
            * chasis.mass / Time.deltaTime / controller.div) * -dir;

        Vector3 forceApply = Vector3.ProjectOnPlane(force * wheelVisual.transform.forward,
            surfaceNormal).normalized * (force * wheelVisual.transform.forward).magnitude;

        chasis.AddForceAtPosition(forceApply, 
            wheelVisual.transform.position - wheelVisual.transform.up * wheelRadius);


     //   Debug.DrawLine(wheelVisual.transform.position - wheelVisual.transform.up * wheelRadius,
       //     wheelVisual.transform.position - wheelVisual.transform.up * wheelRadius + forceApply, Color.red);
    }

    void longitudinalCalculations()
    {
        float torque = 0;
        vLong = transform.InverseTransformVector(chasis.GetPointVelocity(transform.position - transform.up * (wheelPositionCurrent + wheelRadius))).x;

        torque += wheelEngineTorque;
        torque -= torqueToRoad(torque);
        torque -= torqueBrakesAndRollingResitance(torque, DrivingAssistManager.abs);

        wheelAngularVelocity += torque / (wheelAngularMass * Mathf.Pow(wheelRadius, 2)) / 2 * Time.deltaTime;
        
        
        torque = 0;
        torque -= torqueToRoad(torque);
        torque -= torqueBrakesAndRollingResitance(torque, DrivingAssistManager.abs);
        wheelAngularVelocity += torque / (wheelAngularMass * Mathf.Pow(wheelRadius, 2)) / 2 * Time.deltaTime;
        
        wheelVisualAngularVelocity = wheelAngularVelocity;

        Vector3 relativeVelocity = wheelVisual.transform.InverseTransformVector(chasis.GetPointVelocity(wheelVisual.transform.position));
        relativeVelocity = new Vector3(relativeVelocity.x, 0, 0);     
        
        Debug.DrawLine(wheelVisual.transform.position, 
            wheelVisual.transform.position + wheelVisual.transform.TransformVector(relativeVelocity), Color.red, 0, false);

        if(explicar)
        {
            resultado.text = string.Format("σ({0}) =", ((wheelAngularVelocity * wheelRadius - vLong) / Mathf.Abs(vLong)).ToString("F2"));
            numerador.text = string.Format("ω({0})*R({1}) - vLong({2})",
                wheelAngularVelocity.ToString("F2"),
                wheelRadius.ToString("F1"),
                vLong.ToString("F2"));
            denominador.text = string.Format("|vLong|({0})", vLong.ToString("F2"));
        }
    }

    void runSteering()
    {
        if (steeringHelper == null && (DrivingAssistManager.autoCountersteer || DrivingAssistManager.steeringRestriction))
        {
       //     Debug.LogError("No hay SteeringHelper");
            return;
        }


        float actualMaxSteering;
        float actualMinSteering;

        float steeringMidpoint;

        if(DrivingAssistManager.steeringRestriction)
        {            
            actualMaxSteering = steeringHelper.steeringMaxAngle;
            actualMinSteering = -steeringHelper.steeringMaxAngle;
        }
        else
        {
            actualMaxSteering = wheelMaxSteering;
            actualMinSteering = -wheelMaxSteering;
        }

        if (DrivingAssistManager.autoCountersteer)
        {
            steeringMidpoint = steeringHelper.steeringZeroAngle;
        }
        else
            steeringMidpoint = 0;

        actualMaxSteering = Mathf.Max(Mathf.Abs(actualMaxSteering), Mathf.Abs(actualMinSteering));
        actualMinSteering = -Mathf.Max(Mathf.Abs(actualMaxSteering), Mathf.Abs(actualMinSteering));

        float targetAngle = 0;

        if (controller.controls.steering == 0)
            targetAngle = steeringMidpoint;
        if (controller.controls.steering > 0)
            targetAngle = steeringMidpoint + (actualMaxSteering - steeringMidpoint) * Mathf.Abs(controller.controls.steering);
        if(controller.controls.steering < 0)
            targetAngle = steeringMidpoint + (actualMinSteering - steeringMidpoint) * Mathf.Abs(controller.controls.steering);

        targetAngle = Mathf.Clamp(targetAngle, -wheelMaxSteering, wheelMaxSteering);

        wheelSteeringAngle += Mathf.Clamp(targetAngle - wheelSteeringAngle, -wheelSteeringSpeed, wheelSteeringSpeed);
    }

    float torqueBrakesAndRollingResitance(float torque, bool abs)
    {
        int dir = wheelAngularVelocity > 0 ? 1 : -1;
        float maxBreaking = (Mathf.Abs((wheelAngularMass * Mathf.Pow(wheelRadius, 2) / 2) * Mathf.Abs(wheelAngularVelocity)) + torque * Time.deltaTime) / Time.deltaTime;   

        if(handBrake && controller.controls.handBrake)
        {
            return Mathf.Clamp(handBrakeForce + wheelRollingResistance,
            -maxBreaking, maxBreaking) * dir;
        }

        if(abs && wheelAngularVelocity > absMinVelocity)
        {
            float optimalBrakingSpeed = (Mathf.Abs(vLong) - vLong * absTargetSlip) / wheelRadius;
            if (Mathf.Abs(wheelAngularVelocity) < Mathf.Abs(optimalBrakingSpeed) && dir > 0)
                return wheelRollingResistance;
            if (Mathf.Abs(wheelAngularVelocity) < Mathf.Abs(optimalBrakingSpeed) && dir > 0)
                return wheelRollingResistance;
            //maxBreaking = Mathf.Abs((wheelAngularMass * Mathf.Pow(wheelRadius, 2) / 2) * (wheelAngularVelocity - optimalBrakingSpeed) + torque) / Time.deltaTime;
        }
        return Mathf.Clamp(
            controller.controls.brakes * brakeForce + wheelRollingResistance,
            -maxBreaking, maxBreaking) * dir;
    }

    float torqueToRoad(float torque)
    {
        if (vLong != 0)
            wheelSlipRatio = (wheelAngularVelocity * wheelRadius - vLong) / Mathf.Abs(vLong);
        else
            wheelSlipRatio = 0;

        int n = 1;
        if(wheelSlipRatio < 0)
        {
            n = -1;
            wheelSlipRatio *= -1;
        }

        float force = calculateSlipRatioCurveValues() * slipRatioCurveScale * wheelLoad * n * surfaceGrip;

        if (explicarSlipRatio)
        {
            formulaEnPantalla[0].text = force.ToString("F2");
            formulaEnPantalla[1].text = calculateSlipRatioCurveValues().ToString("F2");
            formulaEnPantalla[2].text = wheelSlipRatio.ToString("F2");
            formulaEnPantalla[3].text = slipRatioCurveScale.ToString();
            formulaEnPantalla[4].text = wheelLoad.ToString("F2");
            formulaEnPantalla[5].text = n.ToString();
            formulaEnPantalla[6].text = surfaceGrip.ToString("F2");
        }
        //Limitar fuerza            
        float velocityDiference = wheelAngularVelocity * wheelRadius - vLong;
        float maxForce = velocityDiference / wheelRadius * wheelAngularInertia / Time.deltaTime;

        if (limitar)
        {
            if (Mathf.Abs(maxForce) < Mathf.Abs(force))
            {
                force = Mathf.Abs(maxForce) * n;
            }
        
            if(explicarLimitar)
            {
                formulaDeExplicarEnPantalla[0].text = velocityDiference.ToString("F2");
                formulaDeExplicarEnPantalla[1].text = wheelAngularVelocity.ToString("F2");
                formulaDeExplicarEnPantalla[2].text = wheelRadius.ToString();
                formulaDeExplicarEnPantalla[3].text = vLong.ToString("F2");
                formulaDeExplicarEnPantalla[4].text = wheelAngularInertia.ToString("F2");
                formulaDeExplicarEnPantalla[5].text = wheelAngularMass.ToString();
                formulaDeExplicarEnPantalla[6].text = wheelRadius.ToString();
                formulaDeExplicarEnPantalla[7].text = maxForce.ToString("F2");
                formulaDeExplicarEnPantalla[8].text = velocityDiference.ToString("F2");
                formulaDeExplicarEnPantalla[9].text = wheelRadius.ToString("F2");
                formulaDeExplicarEnPantalla[10].text = wheelAngularInertia.ToString("F2");
                formulaDeExplicarEnPantalla[11].text = Time.deltaTime.ToString("F2");                
            }
        }


        Vector3 forceApply = Vector3.ProjectOnPlane(wheelVisual.transform.right * force, surfaceNormal).normalized * (wheelVisual.transform.right * force).magnitude;
        chasis.AddForceAtPosition(forceApply, transform.position - transform.up * (wheelPositionCurrent + wheelRadius));

     //   Debug.DrawLine(wheelVisual.transform.position - transform.up * wheelRadius,
       //     wheelVisual.transform.position - transform.up * wheelRadius + forceApply, Color.green);

        return force * wheelRadius;
    }

    void initializeParticles()
    {
        surfacesInstantiatedParticleSystem = new ParticleSystem[surfacesParticleControllers.Length];

        for (int i = 0; i < surfacesParticleControllers.Length; i++)
        {
            GameObject particleContainer = Instantiate(surfacesParticleControllers[i], wheelVisual.particleContainer, false);
            surfacesInstantiatedParticleSystem[i] = particleContainer.GetComponent<ParticleSystem>();
        }
        
    }

    void runParticles()
    {
        if(wheelLoad > minEmitWheelLoad)
        emitParticles(surfacesInstantiatedParticleSystem[currentSurface], surfaceEmissionMinSlip[currentSurface],
           surfaceEmissionMaxSlip[currentSurface], 
           (wheelVisual.transform.right * wheelAngularVelocity * wheelRadius - chasis.GetPointVelocity(wheelVisual.transform.position)).magnitude
           , surfaceEmissionMax[currentSurface]);
    }

    void emitParticles(ParticleSystem particles, float minSlip, float maxSlip, float slip, int maxAmmount)
    {
        int ammountToEmit = (int)Mathf.Clamp((slip - minSlip) / (maxSlip - minSlip) * maxAmmount, 0, maxAmmount);
        particles.Emit(ammountToEmit);
    }

    float calculateHookesLaw(float x, float k)
    {
        return x * k;
    }

    float calculateDistanceToGround()
    {
        RaycastHit surface;

        if (Physics.Raycast(transform.position, -transform.up, out surface, wheelPositionMax + 1))
        {
            surfaceNormal = surface.normal;

            string tag = surface.transform.tag;
            if (surfacesTags.Length > 0)
                for (int a = 0; a < surfacesTags.Length; a++)
                {
                    if (surfacesTags[a] == tag)
                    {
                        currentSurface = a;
                        return surface.distance;
                    }
                }

            currentSurface = 0;
            return surface.distance;
        }
        return wheelPositionMax + 1;
    }

    float calculateSlipRatioCurveValues()
    {
        return interpolateCurves(slipRatioCurves, slipRatioCurveAngleValues, wheelSlipAngle, wheelSlipRatio);
    }

    float calculateSlipAngleCurveValues()
    {
        return interpolateCurves(slipAngleCurves, slipAngleCurveSlipValues, wheelSlipRatio, wheelSlipAngle);
    }

    float interpolateCurves(AnimationCurve[] curves, float[] curveValues, float interpolationValue, float value)
    {


        if(curveValues.Length != curves.Length)
        {
         //   Debug.LogError("El numero de curvas no coincide con el numero de valores de curvas");
            return 0;
        }

        for (int i = 0; i < curveValues.Length; i++)
        {
            if (value > curveValues[i])
            {
                if (i + 1 > curveValues.Length)
                    return curves[i].Evaluate(value);

                float topCurveValue = curveValues[i + 1];
                float minCurveValue = curveValues[i];

                interpolationValue = Mathf.Clamp(interpolationValue, minCurveValue, topCurveValue);

                float percent = (interpolationValue - minCurveValue) / (topCurveValue - minCurveValue);

                return curves[i].Evaluate(value) + (curves[i + 1].Evaluate(value) - curves[i].Evaluate(value)) * percent;
            }
        }

        return curves[0].Evaluate(value);
    }

}
