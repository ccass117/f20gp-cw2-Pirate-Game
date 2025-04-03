using UnityEngine;

//pretty much just syncs the head with the body of the megalodon and works out any offsets, nothing much to say here
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