using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameSceneManager : MonoBehaviour
{

    public Transform MainMenuStructure;
    public Transform LoadingStructure;
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void LoadSceneNumber(int id)
    {
        MainMenuStructure.gameObject.SetActive(false);
        LoadingStructure.gameObject.SetActive(true);
        SceneManager.LoadSceneAsync(id, LoadSceneMode.Single);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
