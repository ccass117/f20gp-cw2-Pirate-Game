using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{

    private GameObject mainBossObject;
    private GameObject bossHealthObject;
    public GameObject bossIcon;
    private Health bossHealthScript;
    public Sprite megIcon, krakenIcon, hydraIcon;
    public RectTransform healthBar;

    private float maxX = 380f; // Full health position
    private float minX = 0f;   // Empty health position
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
     
        

        mainBossObject = GameObject.FindGameObjectWithTag("Boss");
        Image bossImage = bossIcon.GetComponent<Image>();
        if (mainBossObject.name == "Megalodon")
        {


            bossImage.sprite = megIcon;


            bossHealthObject = mainBossObject.transform.Find("Body")?.gameObject;

            if (bossHealthObject != null)
            {
                bossHealthScript = bossHealthObject.GetComponent<Health>();

            }

        } else if (mainBossObject.name == "Kraken") {
            bossImage.sprite = krakenIcon;
            bossHealthScript = mainBossObject.GetComponent<Health>();
        } else if (mainBossObject.name == "Hydra")
        {
            bossImage.sprite = hydraIcon;
            bossHealthScript = mainBossObject.GetComponent<Health>();
        }
    }

    void Update()
    {
        if (bossHealthScript != null && healthBar != null)
        {
            float healthPercentage = bossHealthScript.currentHealth / bossHealthScript.maxHealth;

            // Target values
            float targetX = Mathf.Lerp(minX, maxX, healthPercentage);
            float targetWidth = Mathf.Lerp(771f, 0f, healthPercentage);

            // Smoothly interpolate position and width
            float newX = Mathf.Lerp(healthBar.anchoredPosition.x, targetX, Time.deltaTime * 5f);
            float newWidth = Mathf.Lerp(healthBar.sizeDelta.x, targetWidth, Time.deltaTime * 5f);

            // Apply changes
            healthBar.anchoredPosition = new Vector2(newX, healthBar.anchoredPosition.y);
            healthBar.sizeDelta = new Vector2(newWidth, healthBar.sizeDelta.y);
        }
    }
}
