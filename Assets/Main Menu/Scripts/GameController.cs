using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
public class GameController : MonoBehaviour
{
    private static UIDocument document;
    private AudioSource audioSource;
    public VisualElement MenuWindow { get; private set; }
    public Button ContinueButton { get; private set; }
    public Button QuitButton { get; private set; }
    public bool isMenuOpen = false;
    private float defaultVolume = 0.194f;
    private PlayerMove pmove;

    private void Awake()
    {
        QuitButton = document.rootVisualElement.Q<Button>("quit");
        QuitButton.clicked += QuitGame;
        document = GetComponent<UIDocument>();
        MenuWindow = document.rootVisualElement.Q<VisualElement>("Root");
        MenuWindow.style.opacity = isMenuOpen ? 1 : 0;
        audioSource = GetComponent<AudioSource>();
        defaultVolume = audioSource.volume;
        pmove = FindObjectOfType<PlayerMove>();
        ContinueButton = document.rootVisualElement.Q<Button>("continue");
        ContinueButton.clicked += () => CloseMenu();
        if (isMenuOpen)

        {
            OpenMenu();
        }
        else
        {
            CloseMenu();
        }
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (isMenuOpen)
            {
                CloseMenu();
            }
            else
            {
                OpenMenu();
            }
        }
    }

    void OpenMenu()
    {
        MenuWindow.style.opacity = 1;
        UnityEngine.Cursor.lockState = CursorLockMode.Confined;
        isMenuOpen = true;
        audioSource.volume = defaultVolume;
        pmove.isLocked = true;
    }

    void CloseMenu()
    {
        MenuWindow.style.opacity = 0;
        UnityEngine.Cursor.lockState =  CursorLockMode.Locked;
        isMenuOpen = false;
        audioSource.volume = 0;
        pmove.isLocked = false;
    }

    private void QuitGame()
    {
        Debug.Log("OK");
        Application.Quit();
    }
}
