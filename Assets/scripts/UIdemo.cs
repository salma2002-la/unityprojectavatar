using UnityEngine;
using TMPro;

public class UIdemo : MonoBehaviour
{
    public TMP_Text output;
    public TMP_InputField userName;
    public AvatarAI_Ollama ai;
    private ChatUIManager uiManager;

void Start()
{
    uiManager = gameObject.AddComponent<ChatUIManager>();
    uiManager.chatLog = output;
uiManager.AppendMessage("System", "Chat UI is working!");
}


    public void OnSendButton()
    {
        string userText = userName.text;

        if (string.IsNullOrEmpty(userText))
        {
            output.text += "\nYou must type something first!";
            return;
        }

        // 👤 Affiche ce que le joueur dit
        uiManager.AppendMessage("You", userText);

        // 🧠 Appelle l'IA (version UI)
        StartCoroutine(ai.HandleUserMessageUI(userText, uiManager));

        userName.text = ""; // reset input
    }
}
