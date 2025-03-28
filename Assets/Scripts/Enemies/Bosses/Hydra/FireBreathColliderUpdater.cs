using UnityEngine;

public class FireBreathColliderUpdater : MonoBehaviour
{
    private BoxCollider boxCollider;
    private Renderer effectRenderer;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        effectRenderer = GetComponentInChildren<Renderer>();
        if (boxCollider == null)
            Debug.LogWarning("FireBreathColliderUpdater: No BoxCollider found on " + gameObject.name);
        if (effectRenderer == null)
            Debug.LogWarning("FireBreathColliderUpdater: No Renderer found on " + gameObject.name);
    }

    void Update()
    {
        if (boxCollider != null && effectRenderer != null)
        {
            Bounds localBounds = effectRenderer.localBounds;
            boxCollider.size = localBounds.size;
            boxCollider.center = localBounds.center;
        }
    }
}
