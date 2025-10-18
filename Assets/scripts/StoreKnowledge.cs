using System;
using UnityEngine;

[Serializable]
public class StoreKnowledge
{
    public CategoryEntry[] categories;
    public PageEntry[] pages;
    public FAQEntry[] faq;

    public static StoreKnowledge Load()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("storeKnowledge");
        if (jsonFile != null)
        {
            Debug.Log("📄 storeKnowledge.json found!");
            Debug.Log("📄 Raw JSON: " + jsonFile.text);

            StoreKnowledge data = JsonUtility.FromJson<StoreKnowledge>(jsonFile.text);
            if (data == null)
            {
                Debug.LogError("⚠️ Parsed StoreKnowledge is NULL.");
                return null;
            }

            Debug.Log("✅ StoreKnowledge loaded successfully!");
            return data;
        }

        Debug.LogError("❌ storeKnowledge.json not found in Resources!");
        return null;
    }
}

[Serializable]
public class CategoryEntry
{
    public string name;
    public string[] items;
}

[Serializable]
public class PageEntry
{
    public string key;
    public string url;
}

[Serializable]
public class FAQEntry
{
    public string question;
    public string answer;
}
