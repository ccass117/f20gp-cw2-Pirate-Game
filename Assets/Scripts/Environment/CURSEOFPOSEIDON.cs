using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WindEventDisplay : MonoBehaviour
{
    public static WindEventDisplay Instance;

    [Tooltip("UI Text element to display the wind event message")]
    public TextMeshProUGUI messageText;
    [Tooltip("How long the message remains fully visible (in seconds)")]
    public float displayDuration = 3f;
    [Tooltip("Duration of the fade in/out (in seconds)")]
    public float fadeDuration = 1f;

    void Awake()
    {
        // Singleton pattern for easy access
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Make sure messageText starts invisible.
        if (messageText != null)
        {
            Color c = messageText.color;
            c.a = 0f;
            messageText.color = c;
        }
    }

    public void ShowMessage(string message)
    {
        if (messageText == null)
            return;

        StopAllCoroutines();
        StartCoroutine(FadeMessageRoutine(message));
    }

    private IEnumerator FadeMessageRoutine(string message)
    {
        messageText.text = message;
        Color c = messageText.color;
        
        // Fade in
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float normalizedTime = t / fadeDuration;
            messageText.color = new Color(c.r, c.g, c.b, Mathf.Lerp(0f, 1f, normalizedTime));
            yield return null;
        }
        messageText.color = new Color(c.r, c.g, c.b, 1f);
        
        // Hold message
        yield return new WaitForSeconds(displayDuration);
        
        // Fade out
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float normalizedTime = t / fadeDuration;
            messageText.color = new Color(c.r, c.g, c.b, Mathf.Lerp(1f, 0f, normalizedTime));
            yield return null;
        }
        messageText.color = new Color(c.r, c.g, c.b, 0f);
    }
}