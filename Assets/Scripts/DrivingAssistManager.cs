using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrivingAssistManager : MonoBehaviour
{
    public static bool abs = true;
    public static bool autoCountersteer = true;
    public static bool steeringRestriction = true;

    public Toggle absToggle;
    public Toggle counterSteerToggle;
    public Toggle steeringRestrictionToggle;

    private void Start()
    {
        absToggle.isOn = abs;
        counterSteerToggle.isOn = autoCountersteer;
        steeringRestrictionToggle.isOn = steeringRestriction;
    }
    public static void setABS(bool ABS)
    {
        abs = ABS;
    }

    public static void setCountersteer(bool Counter)
    {
        autoCountersteer = Counter;
    }

    public static void setSteeringLimit(bool limit)
    {
        steeringRestriction = limit;
    }
}
