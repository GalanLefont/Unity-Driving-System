using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UICarDebbuger : MonoBehaviour
{
    public static UICarDebbuger main;

    public Rigidbody chasis;
    public SuspensionAndWheel[] wheels;
    public UIWheelDebbuger[] wheelDebuggers;
    public TextMeshProUGUI totalWeightText;

    public float slipMax;
    public float torqueMax;
    public float loadMax;

    
    float torque;
    float hp;
    float wheelTorque;
    private void Awake()
    {
        main = this;
    }

    void Start()
    {
        if(chasis != null)
            loadMax = -chasis.mass * Physics.gravity.y;
        else
            loadMax = 1000 * Physics.gravity.y;
    }

    // Update is called once per frame
    void Update()
    {
        float totalLoad = 0;
        for(int i = 0; i < wheels.Length; i ++)
        {
            wheelDebuggers[i].slip = wheels[i].wheelSlipRatio / slipMax;
            wheelDebuggers[i].torque = wheels[i].wheelEngineTorque / torqueMax;
            wheelDebuggers[i].load = wheels[i].wheelLoad / loadMax;
            totalLoad += wheels[i].wheelLoad;
        }

        totalWeightText.text = string.Format("Total Load: {0}\nEngine Torque: {1}\nHorse Power: {2}\n", totalLoad/-Physics.gravity.y, (int)torque, (int)hp); 
    }

    public void updateDebug(float ntorque, float rpm, float gearRatio)
    {
        torque = ntorque;
        hp = ntorque * rpm / 5252;
        wheelTorque = ntorque * gearRatio;
    }
}
