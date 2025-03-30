using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StandardUI : MonoBehaviour
{
    private GameObject player;
    private Health playerHealth;
    public TextMeshProUGUI healthText;
    public GameObject anchorImg;
    public ShipController shipController;
    private RectTransform anchorRect;
    private float anchorStartY = 1080f;
    private float anchorTargetY = 0f;
    private float moveSpeed = 1f;
    private float elapsedTime = 0f;
    private bool movingDown = false;
    private bool movingUp = false;
    private bool anchorDropped = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerHealth = player.GetComponent<Health>();
        shipController = player.GetComponent<ShipController>();
        anchorRect = anchorImg.GetComponent<RectTransform>();
    }

    void Update()
    {
        healthText.text = (Mathf.Round(playerHealth.currentHealth) + "/" + playerHealth.maxHealth);
        if (shipController.isRaisingAnchor && !movingUp)
        {
            movingUp = true;
            movingDown = false;
            elapsedTime = 0f;
            moveSpeed = shipController.anchorRaiseTime;
            anchorDropped = false;
        }
        else if (shipController.anchored && !movingDown && !anchorDropped && !movingUp)
        {
            movingDown = true;
            elapsedTime = 0f;
        }

        if (movingDown)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / 1f); // 1 second duration
            anchorRect.anchoredPosition = new Vector2(anchorRect.anchoredPosition.x, Mathf.Lerp(anchorStartY, anchorTargetY, t));
            if (t >= 1f)
            {
                movingDown = false;
                anchorDropped = true;
            }
        }

        if (movingUp)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / moveSpeed);
            anchorRect.anchoredPosition = new Vector2(anchorRect.anchoredPosition.x, Mathf.Lerp(anchorTargetY, anchorStartY, t));
            if (t >= 1f) movingUp = false;
        }
    }
}