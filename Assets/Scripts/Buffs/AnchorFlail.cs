using UnityEngine;

public class AnchorFlail : MonoBehaviour
{
    public float rotationSpeed = 360f; // 360 degrees per second (1 rotation per second)
    private ShipController shipController;
    private Transform objTransform;
    private GameObject playerObject;
    private Transform playerTransform;

    void Start()
    {
        objTransform = transform;
        playerObject = GameObject.FindGameObjectWithTag("Player");
        shipController = playerObject.GetComponent<ShipController>();
        playerTransform = playerObject.transform;
    }

    void Update()
    {
        // Rotate the object around the Y axis
        objTransform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        objTransform.position = new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z);

        // Check if the parent has ShipController and update position accordingly
        if (shipController != null && playerTransform != null)
        {
            float targetY = shipController.anchored ? -50f : 5f;
            Vector3 newPosition = new Vector3(playerTransform.position.x, targetY, playerTransform.position.z);
            objTransform.position = newPosition;
        }
    }
}