using System.Collections;
using UnityEngine;

public class AvatarExpressionDemo : MonoBehaviour
{
    SkinnedMeshRenderer faceRenderer;

    void Start()
    {
        faceRenderer = GetComponent<SkinnedMeshRenderer>();
        StartCoroutine(DemoExpressions());
    }

    void SetBlendShape(string name, float value)
    {
        int index = faceRenderer.sharedMesh.GetBlendShapeIndex(name);
        if (index >= 0)
            faceRenderer.SetBlendShapeWeight(index, value);
    }

    IEnumerator DemoExpressions()
    {
        while (true)
        {
            // Neutral
            ResetAll();
            yield return new WaitForSeconds(1f);

            // Smile
            ResetAll();
            SetBlendShape("Fcl_ALL_Joy", 100);
            yield return new WaitForSeconds(2f);

            // Blink
            ResetAll();
            SetBlendShape("Fcl_EYE_Close", 100);
            yield return new WaitForSeconds(1f);

            // Surprised
            ResetAll();
            SetBlendShape("Fcl_ALL_Surprised", 100);
            yield return new WaitForSeconds(2f);

            // Sad
            ResetAll();
            SetBlendShape("Fcl_ALL_Sorrow", 100);
            yield return new WaitForSeconds(2f);
        }
    }

    void ResetAll()
    {
        for (int i = 0; i < faceRenderer.sharedMesh.blendShapeCount; i++)
        {
            faceRenderer.SetBlendShapeWeight(i, 0);
        }
    }
    


}
