using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class CarAndTrackSelection : MonoBehaviour
{
    public static int selectedCar;
    public static int selectedTrack;

    public Color UIHighlightColor;

    public Image[] cars;
    public Image[] tracks;
    public string[] trackNames = { "Fumonya Race Track", "Monte Malo" };

    public MainMenuCar spawner;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        updateUI();
    }

    public void changeCar(int car)
    {
        selectedCar = car;
        if(spawner != null)
            spawner.changeCar(car);
    }

    public void changeTrack(int track)
    {
        selectedTrack = track;
    }

    public void updateUI()
    {
        for(int i = 0; i < cars.Length; i++)
        {
            cars[i].color = i == selectedCar ? UIHighlightColor : new Color(0, 0, 0, 0);
        }

        for (int i = 0; i < tracks.Length; i++)
        {
            tracks[i].color = i == selectedTrack ? UIHighlightColor : new Color(0, 0, 0, 0);
        }
    }

    public void applyChanges()
    {
        PauseMenUController.pause = false;
        if(SceneManager.GetActiveScene().name == trackNames[selectedTrack])
        {
            VehicleSpawner.main.changeVehicle(selectedCar);
        }
        else
        {
            SceneManager.LoadScene(trackNames[selectedTrack]);
        }
    }
}
