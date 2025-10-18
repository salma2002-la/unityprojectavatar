using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class AvatarAI_Ollama : MonoBehaviour
{
    public string ollamaUrl = "http://localhost:11434/api/generate";
    public string modelName = "mistral"; // tu peux mettre llama2, gemma, etc.

    private StoreKnowledge knowledge;

    void Start()
    {
        // Charger les données locales
        knowledge = StoreKnowledge.Load();
        if (knowledge != null)
            Debug.Log("StoreKnowledge ready!");
        else
            Debug.LogError("StoreKnowledge not loaded!");

        // 🧪 Tests initiaux (console)
        StartCoroutine(HandleUserMessage("Tell me about shipping"));
        StartCoroutine(HandleUserMessage("Do you have coats?"));
        StartCoroutine(HandleUserMessage("Go to cart page"));
        StartCoroutine(HandleUserMessage("my name is salma, is it nice?"));
    }

    // --- VERSION CONSOLE ---
    IEnumerator HandleUserMessage(string input)
    {
        Debug.Log("User: " + input);

        // Vérifier la base locale avant Ollama
        string localReply = CheckStoreKnowledge(input);
        if (!string.IsNullOrEmpty(localReply))
        {
            Debug.Log("AI Reply (Local): " + localReply);
            yield break;
        }

        // Sinon → envoyer à Ollama
        yield return StartCoroutine(SendOllamaRequest(input));
    }

    IEnumerator SendOllamaRequest(string input)
    {
        // On échappe proprement les caractères spéciaux
        string safeInput = EscapeJsonString(input);

        string jsonBody = "{\"model\": \"" + modelName + "\", \"prompt\": \"" + safeInput + "\", \"stream\": true}";
        Debug.Log("📤 JSON envoyé à Ollama: " + jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(ollamaUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("❌ Ollama error: " + request.error);
            }
            else
            {
                string rawResponse = request.downloadHandler.text;
                string[] chunks = rawResponse.Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

                StringBuilder finalReply = new StringBuilder();
                foreach (string chunk in chunks)
                {
                    try
                    {
                        OllamaResponse resp = JsonUtility.FromJson<OllamaResponse>(chunk);
                        if (!string.IsNullOrEmpty(resp.response))
                            finalReply.Append(resp.response);
                    }
                    catch
                    {
                        Debug.LogWarning("⚠️ Parsing failed for: " + chunk);
                    }
                }

                Debug.Log("✅ AI Reply (Ollama): " + finalReply.ToString().Trim());
            }
        }
    }

    // --- VERSION UI ---
    public IEnumerator HandleUserMessageUI(string input, ChatUIManager ui)
    {
        string localReply = CheckStoreKnowledge(input);
        if (!string.IsNullOrEmpty(localReply))
        {
            ui.AppendMessage("AI", localReply);
            yield break;
        }

        yield return StartCoroutine(SendOllamaRequestUI(input, ui));
    }

    IEnumerator SendOllamaRequestUI(string input, ChatUIManager ui)
    {
        // 🔹 Contexte enrichi avec ton site
        string siteContext = "You are a helpful assistant for the website 'MyShop.com'. " +
                             "You can answer about products, shipping, and support. " +
                             "If you don't find the information in our store knowledge, answer politely and generally.";

        if (knowledge != null)
        {
            siteContext += "\nHere is part of our store knowledge:\n";

            if (knowledge.faq != null)
            {
                foreach (var q in knowledge.faq)
                    siteContext += $"- Q: {q.question} → A: {q.answer}\n";
            }

            if (knowledge.categories != null)
            {
                foreach (var cat in knowledge.categories)
                    siteContext += $"- Category: {cat.name} (items: {string.Join(", ", cat.items)})\n";
            }
        }

        string fullPrompt = siteContext + "\n\nUser: " + input + "\nAI:";

        // ✅ Échappe la chaîne avant envoi
        string safePrompt = EscapeJsonString(fullPrompt);
        string jsonBody = "{\"model\": \"" + modelName + "\", \"prompt\": \"" + safePrompt + "\", \"stream\": false}";

        Debug.Log("📤 JSON envoyé à Ollama (UI): " + jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(ollamaUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                ui.AppendMessage("Error", "Ollama error: " + request.error);
                Debug.LogError("❌ Ollama error: " + request.error);
            }
            else
            {
                string rawResponse = request.downloadHandler.text;
                try
                {
                    OllamaResponse resp = JsonUtility.FromJson<OllamaResponse>(rawResponse);
                    ui.AppendMessage("AI", !string.IsNullOrEmpty(resp.response)
                        ? resp.response.Trim()
                        : rawResponse);
                }
                catch
                {
                    ui.AppendMessage("AI", rawResponse);
                }
            }
        }
    }

    // --- 🔹 Fonction utilitaire pour sécuriser le texte JSON ---
    string EscapeJsonString(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";
        return input.Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\n", "\\n")
                    .Replace("\r", "\\r");
    }

    // --- Vérifie la base locale JSON ---
    string CheckStoreKnowledge(string input)
    {
        if (knowledge == null) return null;

        string lower = input.ToLowerInvariant();

        if (knowledge.faq != null)
        {
            foreach (var q in knowledge.faq)
            {
                if (!string.IsNullOrEmpty(q.question) && lower.Contains(q.question.ToLowerInvariant()))
                    return q.answer;
            }
        }

        if (knowledge.categories != null)
        {
            foreach (var cat in knowledge.categories)
            {
                if (!string.IsNullOrEmpty(cat.name) && lower.Contains(cat.name.ToLowerInvariant()))
                {
                    string items = cat.items != null ? string.Join(", ", cat.items) : "no items listed";
                    return $"Yes! We have {items} available in {cat.name}.";
                }
            }
        }

        if (knowledge.pages != null)
        {
            foreach (var page in knowledge.pages)
            {
                if (!string.IsNullOrEmpty(page.key) && lower.Contains(page.key.ToLowerInvariant()))
                    return $"You can visit the {page.key} page here: {page.url}";
            }
        }

        return null;
    }
}

[System.Serializable]
public class OllamaResponse
{
    public string response;
    public bool done;
}
