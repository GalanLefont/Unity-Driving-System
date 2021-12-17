using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWheelDebbuger : MonoBehaviour
{
    public float slip;
    public float torque;
    public float load;

    public Slider slipSlider;
    public Slider torqueSlider;
    public Slider loadSlider;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(slipSlider != null)
            slipSlider.value = slip;
        if(torqueSlider != null)
            torqueSlider.value = torque;
        if(loadSlider != null)
            loadSlider.value = load;
    }
}
