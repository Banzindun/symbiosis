using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroManager : MonoBehaviour
{
    public GameObject[] screens;

    private int screenIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.Pause();
        NextScreen();
    }

    public void NextScreen()
    {
        if (screenIndex >= screens.Length)
        {
            gameObject.SetActive(false);
            Skip();
            return;
        }

        screens[screenIndex].SetActive(true);
        screenIndex++;
    }

    public void Skip()
    {
        gameObject.SetActive(false);
        GameManager.Instance.OnIntroFinished();
    }

    private void OnDestroy()
    {

    }
}
