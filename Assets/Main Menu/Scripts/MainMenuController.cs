using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    private static UIDocument document;
    public Button NewGameButton { get; private set; }
    public  Button OptionsButton { get; private set; }
    public Button LoadButton { get; private set; }
    public Button ContinueButton { get; private set; }
    public Button QuitButton { get; private set; }
    public VisualElement MenuWindow { get; private set; }
    public VisualElement LoadingWindow { get; private set; }
    public Label LoadingText { get; private set; }

    private void Awake()
    {
        document = GetComponent<UIDocument>();

        NewGameButton = document.rootVisualElement.Q<Button>("new-game");
        NewGameButton.clicked += () => NewGame();

        MenuWindow = document.rootVisualElement.Q<VisualElement>("Content");
        LoadingWindow = document.rootVisualElement.Q<VisualElement>("LoadingHUD");
        LoadingText = document.rootVisualElement.Q<Label>("loading-text");


        QuitButton = document.rootVisualElement.Q<Button>("quit");
        QuitButton.clicked += QuitGame;
    }

    private void NewGame()
    {
        StartCoroutine(LoadMainGameScene());
    }

    private IEnumerator LoadMainGameScene()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(1);
        LoadingWindow.style.display = DisplayStyle.Flex;
        MenuWindow.style.display = DisplayStyle.None;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            LoadingText.text = "Loading " + ((int)progress * 100);
            yield return null;
        }
        yield break;
    }


    private void QuitGame()
    {
        Debug.Log("OK");
        Application.Quit();
    }
}
