using UnityEngine;
using System.Collections.Generic;

public class Islands : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        List<List<int>> a = blobGenerator(15, 15, 100);
        printBlob(a);

    }

    // Update is called once per frame
    void Update()
    {

    }

    static List<List<int>> blobGenerator(int maxWidth, int maxHeight, int landMass)
    {
        List<List<int>> tiles = new List<List<int>>();
        List<(int, int)> open = new List<(int, int)>();
        int mass = 0;

        // initialise tile map as empty
        for (int y = 0; y < maxHeight; y++)
        {
            tiles.Add(new List<int>(new int[maxWidth]));
        }

        // spawn land in center
        open.Add((maxWidth / 2, maxHeight / 2));

        while (mass < landMass && open.Count > 0)
        {
            // get random open tile
            int pos = Random.Range(0, open.Count);
            (int, int) cur = open[pos];
            int x = cur.Item1;
            int y = cur.Item2;

            // randomly skip if too far from COM
            (float, float) com = centerOfMass(tiles);
            float comX = com.Item1;
            float comY = com.Item2;
            float dist = (comX - x) * (comX - x) + (comY - y) * (comY - y);
            if (Random.Range(0, dist) > 1) continue;

            // add landmass
            open.RemoveAt(pos);
            tiles[y][x] = 1;
            mass++;

            // add neighbors to open
            // left
            if (x > 0 && tiles[y][x - 1] == 0)
            {
                open.Add((x - 1, y));
            }
            // right
            if (x < maxWidth - 1 && tiles[y][x + 1] == 0)
            {
                open.Add((x + 1, y));
            }
            // up
            if (y > 0 && tiles[y - 1][x] == 0)
            {
                open.Add((x, y - 1));
            }
            // up
            if (y < maxHeight - 1 && tiles[y + 1][x] == 0)
            {
                open.Add((x, y + 1));
            }
        }

        return tiles;
    }

    // find the center of mass of a blob
    static (float, float) centerOfMass(List<List<int>> tiles)
    {
        int count = 0;
        int sumX = 0;
        int sumY = 0;

        // sum together all the positions
        for (int y = 0; y < tiles.Count; y++)
        {
            for (int x = 0; x < tiles[y].Count; x++)
            {
                if (tiles[y][x] == 1)
                {
                    count++;
                    sumX += x;
                    sumY += y;
                }
            }
        }

        // find the average
        if (count == 0)
        {
            return ((float)tiles[0].Count / 2, (float)tiles.Count / 2);
        }
        return ((float)sumX / count, (float)sumY / count);

    }

    // print a blob to the console
    static void printBlob(List<List<int>> tiles)
    {
        for (int y = 0; y < tiles.Count; y++)
        {
            Debug.Log(string.Join(" ", tiles[y]));
        }
    }
}
