using UnityEngine;
using System.Collections.Generic;

public class Islands
{

    // holds the mapping from (baseTileInt, flipHoriz, rotate amount)
    private static Dictionary<int, (int, bool, int)> tileMapping = generateTileMapping();

    public static List<List<(int, bool, int)>> genIsland(int maxWidth, int maxHeight, int mass, float roundedness)
    {
        // generate a 2d "blob"
        List<List<int>> a = blobGenerator(maxWidth, maxHeight, mass, roundedness);

        // generate tile as edge bit encodedints 
        a = generateEdgeInts(a);
        //printBlob(a);

        // map the tiles to base tile + transform (tile, flipHoriz, rotation CW 90º)
        List<List<(int, bool, int)>> b = mapTilesToBase(a);

        return b;
    }


    // map tile ints to base tiles with rotation and flip flags
    static List<List<(int, bool, int)>> mapTilesToBase(List<List<int>> tileInts)
    {
        List<List<(int, bool, int)>> baseTiles = new List<List<(int, bool, int)>>();
        for (int y = 0; y < tileInts.Count; y++)
        {
            baseTiles.Add(new List<(int, bool, int)>());
            for (int x = 0; x < tileInts[0].Count; x++)
            {
                // find the current tile in the tile map and take its base tile + transform
                baseTiles[y].Add(tileMapping[tileInts[y][x]]);
            }
        }
        return baseTiles;
    }


    static Dictionary<int, (int, bool, int)> generateTileMapping()
    {
        Dictionary<int, (int, bool, int)> tileMap = new Dictionary<int, (int, bool, int)>();
        // make sure the base tiles can map to these ints
        // for instance 847 (2 adjacent corners, one with 1, and the other with 2) would map to 
        // # # # #
        // x     #
        // o     x
        // o o o o
        // where # are in-land, x are land connections, o are sea

        int[] tileInts = {
            321, // 1 corner, 1 each side
            323, // 1 corner, 1 on one side 2 on the other
            451, // 1 corner, 2 on each side

            839, // 2 adjacent corners, 1 each side
            847, // 2 adjacent corners, 1 on one side 2 on the other
            975, // 2 adjacent corners, 2 each side

            1385, // 2 opposite corners, 1 each side
            1387, // 2 opposite corners, 1 2
            // errors will provide the rest lol
            1535, // 2 opposite corners, 2 each side

            1903, // 3 corners, 1 each side
            1919, // 3 corners, 1 on one side 2 on the other
            2047, // 3 corners, 2 each side
        };

        for (int i = 0; i < tileInts.Length; i++)
        {
            // add all rotations
            int cur = tileInts[i];
            tileMap.TryAdd(cur, (cur, false, 0));
            tileMap.TryAdd(rotateCW(cur), (cur, false, 1));
            tileMap.TryAdd(rotateCW(rotateCW(cur)), (cur, false, 2));
            tileMap.TryAdd(rotateCW(rotateCW(rotateCW(cur))), (cur, false, 3));
            tileMap.TryAdd(flipHoriz(cur), (cur, true, 0));
            tileMap.TryAdd(rotateCW(flipHoriz(cur)), (cur, true, 1));
            tileMap.TryAdd(rotateCW(rotateCW(flipHoriz(cur))), (cur, true, 2));
            tileMap.TryAdd(rotateCW(rotateCW(rotateCW(flipHoriz(cur)))), (cur, true, 3));
        }

        tileMap.Add(4095, (4095, false, 0)); // full tile
        tileMap.Add(0, (0, false, 0)); // empty tile

        return tileMap;
    }

    static int flipHoriz(int tileInt)
    {
        int newTileInt = 0;
        // edges
        newTileInt |= (tileInt & (0b1 << 0)) << 1;
        newTileInt |= (tileInt & (0b1 << 1)) >> 1;
        newTileInt |= (tileInt & (0b11 << 2)) << 4;
        newTileInt |= (tileInt & (0b1 << 4)) << 1;
        newTileInt |= (tileInt & (0b1 << 5)) >> 1;
        newTileInt |= (tileInt & (0b11 << 6)) >> 4;

        // corners
        newTileInt |= (tileInt & (0b1 << 8)) << 1;
        newTileInt |= (tileInt & (0b1 << 9)) >> 1;
        newTileInt |= (tileInt & (0b1 << 10)) << 1;
        newTileInt |= (tileInt & (0b1 << 11)) >> 1;
        return newTileInt;
    }

    static int rotateCW(int tileInt)
    {
        int newTileInt = 0;
        // edges
        newTileInt |= (tileInt & (0b11 << 0)) << 2;
        newTileInt |= (tileInt & (0b1 << 2)) << 3;
        newTileInt |= (tileInt & (0b1 << 3)) << 1;
        newTileInt |= (tileInt & (0b11 << 4)) << 2;
        newTileInt |= (tileInt & (0b1 << 6)) >> 5;
        newTileInt |= (tileInt & (0b1 << 7)) >> 7;

        // corners
        newTileInt |= (tileInt & (0b1 << 8)) << 1;
        newTileInt |= (tileInt & (0b1 << 9)) << 1;
        newTileInt |= (tileInt & (0b1 << 10)) << 1;
        newTileInt |= (tileInt & (0b1 << 11)) >> 3;
        return newTileInt;
    }


    static List<List<int>> generateEdgeInts(List<List<int>> blobTiles)
    {

        // convert the blobs to tiles, the items in blob act as the inner corners of a grid
        // returned dimensions are +1 from blob in both x and y as it goes from corners to squares
        // edges are filled with 0s

        // tiles are stored as a bit string 
        // 9 1  2 10
        // 7      3
        // 8      4
        //12 5  6 11

        List<List<int>> tiles = new List<List<int>>();
        int height = blobTiles.Count + 1;
        int width = blobTiles[0].Count + 1;

        // initialise tile map as empty
        for (int y = 0; y < height; y++)
        {
            tiles.Add(new List<int>(new int[width]));
        }

        // loop over all the tile squares and generate bit strings
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int tileInt = 0;

                // get corner sections, if on edge, default empty
                bool tl = x > 0 && y > 0 ? (blobTiles[y - 1][x - 1] == 1) : false;
                bool tr = x < width - 1 && y > 0 ? (blobTiles[y - 1][x] == 1) : false;
                bool br = x < width - 1 && y < height - 1 ? blobTiles[y][x] == 1 : false;
                bool bl = x > 0 && y < height - 1 ? blobTiles[y][x - 1] == 1 : false;

                //top
                if (y != 0)
                {
                    tileInt |= (tiles[y - 1][x] & (0b11 << 4)) >> 4;
                }
                else
                {
                    //todo
                }

                //left
                if (x != 0)
                {
                    tileInt |= (tiles[y][x - 1] & (0b11 << 2)) << 4;
                }
                else
                {
                    //todo
                }

                // bottm side
                if (br)
                {
                    if (bl)
                    {
                        // both
                        tileInt |= 0b11 << 4;
                    }
                    else
                    {
                        // just br
                        if (Random.Range(0f, 1f) > 0.5)
                        {
                            tileInt |= 0b11 << 4;
                        }
                        else
                        {
                            tileInt |= 0b1 << 5;
                        }
                    }
                }
                else
                {
                    if (bl)
                    {
                        // just bl
                        if (Random.Range(0f, 1f) > 0.5)
                        {
                            tileInt |= 0b11 << 4;
                        }
                        else
                        {
                            tileInt |= 0b1 << 4;
                        }
                    }
                }


                // right side
                if (tr)
                {
                    if (br)
                    {
                        // both
                        tileInt |= 0b11 << 2;
                    }
                    else
                    {
                        // just tr
                        if (Random.Range(0f, 1f) > 0.5)
                        {
                            tileInt |= 0b11 << 2;
                        }
                        else
                        {
                            tileInt |= 0b1 << 2;
                        }
                    }
                }
                else
                {
                    if (br)
                    {
                        // just br
                        if (Random.Range(0f, 1f) > 0.5)
                        {
                            tileInt |= 0b11 << 2;
                        }
                        else
                        {
                            tileInt |= 0b1 << 3;
                        }
                    }
                }

                //add corners to 9, 10, 11, 12 bits
                tileInt |= (tl ? 1 : 0) << 8;
                tileInt |= (tr ? 1 : 0) << 9;
                tileInt |= (br ? 1 : 0) << 10;
                tileInt |= (bl ? 1 : 0) << 11;

                tiles[y][x] = tileInt;
            }
        }
        return tiles;
    }


    static List<List<int>> blobGenerator(int maxWidth, int maxHeight, int landMass, float randomness)
    {
        // generate connected 2d blob in a 2d array

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
            float dist = Mathf.Sqrt((comX - x) * (comX - x) + (comY - y) * (comY - y));
            //if (Random.Range(0, dist) < randomness) continue;

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
