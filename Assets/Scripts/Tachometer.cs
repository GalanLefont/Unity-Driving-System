using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Tachometer : MonoBehaviour
{
    /*
    float maxRPM;
    float rpm;
    int gear;
    float speed;
    */

    public static GameObject UICanvas;

    public TextMeshProUGUI speedText;
    public TextMeshProUGUI rpmText;
    public TextMeshProUGUI gearText;
    
    public Slider tachoBar;

    float RPM;
    float actualRPM;
    float maxRPM;

    float speed;
    int gear;

    float smoothingFactor;
    // Start is called before the first frame update
    void Start()
    {
        UICanvas = transform.parent.gameObject;
    }

    private void FixedUpdate()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        RPM += (actualRPM - RPM) * smoothingFactor * Time.deltaTime / 0.02F;
        if(float.IsNaN(RPM) || float.IsInfinity(RPM))
        {
            RPM = actualRPM;
        }
        setTachoVisual();    
    }

    public void setTacho(float newRPM, float mRPM, int newGear, float nSpeed, float nSmoothingFactor)
    {
        actualRPM = newRPM;
        maxRPM = mRPM;
        speed = nSpeed;
        gear = newGear;
        smoothingFactor = nSmoothingFactor;
    }

    void setTachoVisual()
    {
        speedText.text = string.Format("{0} Ku/h", (int)(speed * 3600 / 1000 * 100) / 100);
        rpmText.text = string.Format("{0} RPM", (int)RPM);
        gearText.text = string.Format("{0} Gear", gear);

        tachoBar.value = RPM / maxRPM;
    }
}
