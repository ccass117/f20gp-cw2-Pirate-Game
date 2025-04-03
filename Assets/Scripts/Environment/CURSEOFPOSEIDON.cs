using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//this is a joke event, basically when wind changes there is a 1 in 1000 change that wind strength is set to 10000 and a message pops up
//we just thought this would be funny, actual random events are in windmgr.cs, this is just called there for the UI event
public class CURSEOFPOSEIDON : MonoBehaviour
{
    public static CURSEOFPOSEIDON Instance;

    public TextMeshProUGUI messageText;
    public float displayDuration = 3f;
    public float fadeDuration = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (messageText != null)
        {
            Color c = messageText.color;
            c.a = 0f;
            messageText.color = c;
        }
    }

    //dispalys the message
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
        
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float normalizedTime = t / fadeDuration;
            messageText.color = new Color(c.r, c.g, c.b, Mathf.Lerp(0f, 1f, normalizedTime));
            yield return null;
        }
        messageText.color = new Color(c.r, c.g, c.b, 1f);
        
        yield return new WaitForSeconds(displayDuration);
        
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float normalizedTime = t / fadeDuration;
            messageText.color = new Color(c.r, c.g, c.b, Mathf.Lerp(1f, 0f, normalizedTime));
            yield return null;
        }
        messageText.color = new Color(c.r, c.g, c.b, 0f);
    }
}