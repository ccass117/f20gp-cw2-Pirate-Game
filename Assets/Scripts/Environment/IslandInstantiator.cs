using UnityEngine;
using System.Collections.Generic;

public class IslandInstantiator : MonoBehaviour
{
    public GameObject[] tileSet;
    private Transform selfT;
    public int maxWidth = 5;
    public int maxHeight = 5;
    public int mass = 10;

    public List<Material> outerMat;
    public List<Material> innerMat;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // get self transform component
        selfT = GetComponent<Transform>();

        //generate island
        List<List<(int, bool, int)>> a = Islands.genIsland(maxWidth, maxHeight, mass, 5f);

        // instantiate the prefabs
        for (int y = 0; y < maxHeight + 1; y++)
        {
            for (int x = 0; x < maxWidth + 1; x++)
            {
                (int, bool, int) cur = a[y][x];
                int id = cur.Item1;
                bool flip = cur.Item2;
                int rotation = cur.Item3;

                // skip empty ones (remove if testing)
                if (id == 0) continue;

                // linear search for prefabs
                GameObject item = tileSet[0];
                bool found = false;

                for (int i = 0; i < tileSet.Length; i++)
                {
                    if (int.Parse(tileSet[i].name) == id)
                    {
                        item = tileSet[i];
                        found = true;
                    }
                }

                // if tile not found, panic
                if (!found)
                {
                    Debug.Log("not found :");
                    Debug.Log(id);
                    continue;
                }

                // actually instantiate the right prefab
                GameObject b = Instantiate(item);
                setMaterials(b);

                // change positions rotation and flip
                b.transform.position = new Vector3(selfT.position.x + x * 6 - (maxWidth * 3), 0, selfT.position.z + y * -6 + (maxHeight * 3));
                if (flip)
                {
                    b.transform.localScale = new Vector3(-1, 1, 1);
                }
                b.transform.rotation = Quaternion.Euler(0, rotation * 90, 0);

            }
        }
    }

    void setMaterials(GameObject island)
    {
        Renderer[] renderers = island.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            // replace all inner mats with inner material
            if (rend.CompareTag("innerMat"))
            {
                rend.SetMaterials(innerMat);
            }
            // replace all outer mats with outer material
            else if (rend.CompareTag("outerMat"))
            {
                rend.SetMaterials(outerMat);
            }
        }

    }
}

