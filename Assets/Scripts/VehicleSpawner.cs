using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleSpawner : MonoBehaviour
{
    public static VehicleSpawner main;

    public GameObject[] vehicles;
    public GameObject currentVehicle;
    public void changeVehicle(int id)
    {
        GameObject newVehicle = Instantiate<GameObject>(vehicles[id], transform);
        Destroy(currentVehicle);
        currentVehicle = newVehicle;
    }
    // Start is called before the first frame update
    void Start()
    {
        main = this;
        GameObject newVehicle = Instantiate<GameObject>(vehicles[CarAndTrackSelection.selectedCar], transform);        
        currentVehicle = newVehicle;
    }

}
