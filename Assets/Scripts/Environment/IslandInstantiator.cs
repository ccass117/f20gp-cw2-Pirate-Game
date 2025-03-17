using UnityEngine;
using System.Collections.Generic;

public class IslandInstantiator : MonoBehaviour
{
    public GameObject[] go;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //generate island

        int h = 10;
        int w = 10;
        List<List<(int, bool, int)>> a = Islands.genIsland(w, h, 30, 5f);
        for (int y = 0; y < h + 1; y++)
        {
            for (int x = 0; x < w + 1; x++)
            {
                (int, bool, int) cur = a[y][x];
                int id = cur.Item1;
                bool flip = cur.Item2;
                int rotation = cur.Item3;
                if (id == 0) continue;

                GameObject item = go[0];
                bool found = false;

                for (int i = 0; i < go.Length; i++)
                {
                    if (int.Parse(go[i].name) == id)
                    {
                        item = go[i];
                        found = true;
                    }
                }

                if (!found)
                {
                    Debug.Log("not found :");
                    Debug.Log(id);
                    continue;
                }
                GameObject b = Instantiate(item);
                b.transform.position = new Vector3(x * 6, 0, y * -6);
                Debug.Log(b.transform.position);
                if (flip)
                {
                    b.transform.localScale = new Vector3(-1, 1, 1);
                }
                b.transform.rotation = Quaternion.Euler(0, rotation * 90, 0);


            }

        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}
