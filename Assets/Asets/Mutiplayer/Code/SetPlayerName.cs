using UnityEngine;
using TMPro;

public class SetPlayerName : MonoBehaviour
{
    public TMP_InputField input;

    public void SaveName()
    {
        string name = input.text;

        if (string.IsNullOrEmpty(name))
            name = "NoName";

        PlayerPrefs.SetString("PlayerName", name);

        Debug.Log("Tên ?ã l?u: " + name);
    }
}