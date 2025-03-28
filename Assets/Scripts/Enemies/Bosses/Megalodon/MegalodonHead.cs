using UnityEngine;

public class MegalodonHead : MonoBehaviour
{
    public Transform bodyTransform;
    private Rigidbody parentRigidbody;
    private Vector3 positionOffset;
    private Quaternion rotationOffset;
    void Start()
    {
        parentRigidbody = transform.parent.GetComponent<Rigidbody>();
        
        positionOffset = transform.localPosition;
        rotationOffset = transform.localRotation;
    }

    void FixedUpdate()
    {
        if (parentRigidbody != null)
        {
            GetComponent<Rigidbody>().MovePosition(
                parentRigidbody.position + parentRigidbody.transform.TransformDirection(positionOffset)
            );
            GetComponent<Rigidbody>().MoveRotation(
                parentRigidbody.rotation * rotationOffset
            );
        }
    }
}