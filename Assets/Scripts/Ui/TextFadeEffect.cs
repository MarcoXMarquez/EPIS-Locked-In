using UnityEngine;
using TMPro;
using System.Collections;

public class TextFadeEffect : MonoBehaviour
{
    private TextMeshProUGUI text;
    private Coroutine fadeRoutine;

    void Awake() => text = GetComponent<TextMeshProUGUI>();

    public void ShowText(string message, float duration)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeRoutine(message, duration));
    }

    IEnumerator FadeRoutine(string message, float duration)
    {
        text.text = message;
        text.alpha = 1; // Aparece de golpe
        yield return new WaitForSeconds(duration);

        // Desvanecimiento suave
        while (text.alpha > 0)
        {
            text.alpha -= Time.deltaTime;
            yield return null;
        }
    }
}