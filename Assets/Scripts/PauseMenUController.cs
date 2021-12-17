using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenUController : MainMenuController
{
    public static bool pause;

    public GameObject pauseMenu;


    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            activeMenu = 0;
            pause = !pause;

            pauseMenu.SetActive(pause);
            Tachometer.UICanvas.SetActive(!pause);
            GetComponent<MainMenuController>().activeMenu = 0;
        }

        pauseMenu.SetActive(pause);
        Tachometer.UICanvas.SetActive(!pause);

        if (pause)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;
    }
    public void resume()
    {
        pause = false;
        Time.timeScale = 1;
        GetComponent<MainMenuController>().activeMenu = 0;
    }
    public void returnToMainMenu()

    {
        pause = false;
        Time.timeScale = 1;
        SceneManager.LoadScene("Main Menu");
    }
}
