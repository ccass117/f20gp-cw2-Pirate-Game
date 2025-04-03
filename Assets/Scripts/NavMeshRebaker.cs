using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Unity.AI.Navigation;

//rebake the navmesh at runtime, so that procedurally generated islands are included in the navmesh and it is built around them for enemy navigation, called on every level load
public class NavMeshRebaker : MonoBehaviour
{
    private NavMeshSurface surface;

    void Start()
    {
        surface = GetComponent<NavMeshSurface>();
        //gives the islands a second to generate before rebaking the navmesh
        StartCoroutine(delay(1f));
    }

    IEnumerator delay(float delay)
    {
        yield return new WaitForSeconds(delay);
        surface.BuildNavMesh();
    }
}