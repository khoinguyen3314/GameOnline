using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ChatUI : MonoBehaviour
{
    public TMP_InputField inputField;
    public Button sendButton;
    public TextMeshProUGUI chatContent;

    void Start()
    {
        sendButton.onClick.AddListener(SendMessage);
    }

    void SendMessage()
    {
        string text = inputField.text;

        if (!string.IsNullOrWhiteSpace(text))
        {
            ChatManager.Instance.SendChatMessage(text);
            inputField.text = "";
        }
    }
}