using TMPro;
using UnityEngine;

public class ChatUIManager : MonoBehaviour
{
    public TMP_Text chatLog;

    // 🔹 Ajoute du texte à l’output TMP
    public void AppendMessage(string sender, string message)
    {
        if (chatLog != null)
            chatLog.text += $"\n<b>{sender}:</b> {message}";
        
    }
}
