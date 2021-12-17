using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuCar : MonoBehaviour
{
    public GameObject[] cars;

    // Start is called before the first frame update
    void Start()
    {
        int standingCar = (int)Random.value % cars.Length;

        CarAndTrackSelection.selectedCar = standingCar;
        for(int i = 0; i < cars.Length; i++)
        {
            cars[i].SetActive(i == standingCar);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void changeCar(int car)
    {
        for (int i = 0; i < cars.Length; i++)
        {
            if (i == car)
            {
                cars[i].SetActive(true);
                cars[i].transform.position = Vector3.up * 0.7f;
            }
            else
                cars[i].SetActive(false);
        }
    }
}
