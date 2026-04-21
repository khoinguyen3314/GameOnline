using UnityEngine;

public class PlayerNameManager : MonoBehaviour
{
    public static PlayerNameManager Instance;

    public string PlayerName;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}