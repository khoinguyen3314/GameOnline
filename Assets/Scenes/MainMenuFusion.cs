using UnityEngine;
using TMPro;
using Fusion;

public class MainMenuFusion : MonoBehaviour
{
    public TMP_InputField inputName;

    private NetworkRunner runner;

    public void OnClickHost()
    {
        SaveName();
        StartGame(GameMode.Host);
    }

    public void OnClickJoin()
    {
        SaveName();
        StartGame(GameMode.Client);
    }

    void SaveName()
    {
        string name = inputName.text;

        if (string.IsNullOrEmpty(name))
            name = "NoName";

        PlayerPrefs.SetString("PlayerName", name);
    }

    async void StartGame(GameMode mode)
    {
        runner = gameObject.AddComponent<NetworkRunner>();

        await runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "Room1",
            Scene = SceneRef.FromIndex(1),
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }
}