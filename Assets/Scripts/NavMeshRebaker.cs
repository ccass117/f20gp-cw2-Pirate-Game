using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Unity.AI.Navigation;

public class NavMeshRebaker : MonoBehaviour
{
    private NavMeshSurface surface;

    void Start()
    {
        surface = GetComponent<NavMeshSurface>();
        StartCoroutine(delay(0.5f));
    }

    IEnumerator delay(float delay)
    {
        yield return new WaitForSeconds(delay);
        surface.BuildNavMesh();
        Debug.Log("NavMesh rebuilt at runtime.");
    }
}