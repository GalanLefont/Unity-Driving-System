using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public int activeMenu;
    public GameObject[] menues;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < menues.Length; i++)
        {
            menues[i].SetActive(activeMenu == i);
        }
    }

    public void back()
    {
        activeMenu = 0;
    }

    public void settings()
    {
        activeMenu = 1;
    }

    public void credits()
    {
        activeMenu = 2;
    }

    public void quit()
    {
        Application.Quit();
    }

    public void play()
    {
        activeMenu = 3;
        
    }
}
