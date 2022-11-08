/*
 * Copyright (c) 2017 Razeware LLC
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;
    public List<Sprite> Characters = new List<Sprite>();
    public GameObject Tile;
    public int Rows;
    public int Columns;

    private GameObject[,] _tiles;

    public bool IsShifting { get; set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Vector2 offset = Tile.GetComponent<SpriteRenderer>().bounds.size;
        CreateBoard(offset.x, offset.y);
    }

    private void CreateBoard(float xOffset, float yOffset)
    {
        _tiles = new GameObject[Columns, Rows];

        float startX = transform.position.x;
        float startY = transform.position.y;

        var previousLeft = new Sprite[Rows];
        Sprite previousBelow = null;

        // Заполнение идет направо и вверх
        for (int x = 0; x < Columns; x++)
        {
            var posX = startX + (xOffset * x);

            for (int y = 0; y < Rows; y++)
            {
                var posY = startY + (yOffset * y);
                var vector = new Vector3(posX, posY, 0);
                GameObject newTile = Instantiate(Tile, vector, Tile.transform.rotation);
                newTile.transform.parent = transform;

                var possibleCharacters = new List<Sprite>();
                possibleCharacters.AddRange(Characters);
                possibleCharacters.Remove(previousLeft[y]);
                possibleCharacters.Remove(previousBelow);

                Sprite newSprite = possibleCharacters[Random.Range(0, possibleCharacters.Count)];
                newTile.GetComponent<SpriteRenderer>().sprite = newSprite;
                _tiles[x, y] = newTile;

                previousLeft[y] = newSprite;
                previousBelow = newSprite;
            }
        }
    }

    public IEnumerator FindNullTiles()
    {
        for (int x = 0; x < Columns; x++)
        {
            for (int y = 0; y < Rows; y++)
            {
                if (_tiles[x, y].GetComponent<SpriteRenderer>().sprite == null)
                {
                    yield return StartCoroutine(ShiftTilesDown(x, y));
                    break;
                }
            }
        }

        for (int x = 0; x < Columns; x++)
        {
            for (int y = 0; y < Rows; y++)
            {
                _tiles[x, y].GetComponent<Tile>().ClearAllMatches();
            }
        }
    }

    private IEnumerator ShiftTilesDown(int x, int yStart, float shiftDelay = .03f)
    {
        IsShifting = true;
        var renders = new List<SpriteRenderer>();
        int nullCount = 0;

        for (int y = yStart; y < Rows; y++)
        {
            var render = _tiles[x, y].GetComponent<SpriteRenderer>();
            if (render.sprite == null)
                nullCount++;

            renders.Add(render);
        }

        for (int i = 0; i < nullCount; i++)
        {
            GUIManager.Instance.Score += 50;

            yield return new WaitForSeconds(shiftDelay);
            for (int k = 0; k < renders.Count - 1; k++)
            {
                var a = renders[k];
                var b = renders[k + 1];
                a.sprite = b.sprite;
                b.sprite = GetNewSprite(x, Rows - 1);
            }
        }
        IsShifting = false;
    }

    private Sprite GetNewSprite(int x, int y)
    {
        var possibleCharacters = new List<Sprite>();
        possibleCharacters.AddRange(Characters);

        if (x > 0)
            possibleCharacters.Remove(_tiles[x - 1, y].GetComponent<SpriteRenderer>().sprite);

        if (x < Columns - 1)
            possibleCharacters.Remove(_tiles[x + 1, y].GetComponent<SpriteRenderer>().sprite);

        if (y > 0)
            possibleCharacters.Remove(_tiles[x, y - 1].GetComponent<SpriteRenderer>().sprite);

        return possibleCharacters[Random.Range(0, possibleCharacters.Count)];
    }
}
