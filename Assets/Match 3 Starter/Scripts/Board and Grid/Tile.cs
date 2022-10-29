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


public class Tile : MonoBehaviour
{
    private static Color selectedColor = new Color(.5f, .5f, .5f, 1.0f);
    private static Color defaultColor;

    private static Tile previousSelected = null;

    private SpriteRenderer render;
    private bool isSelected = false;

    private Vector2[] adjacentDirections = new Vector2[] {
        Vector2.up, Vector2.down, Vector2.left, Vector2.right
    };

    void Awake()
    {
        render = GetComponent<SpriteRenderer>();
        defaultColor = render.color;
    }

    private void Select()
    {
        isSelected = true;
        render.color = selectedColor;
        previousSelected = gameObject.GetComponent<Tile>();
        SFXManager.instance.PlaySFX(Clip.Select);
    }

    private void Deselect()
    {
        isSelected = false;
        render.color = defaultColor;
        previousSelected = null;
    }

    void OnMouseDown()
    {
        if (render.sprite == null || BoardManager.instance.IsShifting)
            return;

        if (isSelected)
        {
            Deselect();
            return;
        }

        if (previousSelected == null)
        {
            Select();
            return;
        }

        // Поиск соседней фишки
        if (GetAllAdjacentTiles().Contains(previousSelected.gameObject))
        {
            SwapSprite(previousSelected.render);
            previousSelected.ClearAllMatches();
            previousSelected.Deselect();
            ClearAllMatches();
        }
        else
        {
            previousSelected.Deselect();
            Select();
        }
    }

    private GameObject GetAdjacentTile(Vector2 castDir)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, castDir);
        return hit.collider?.gameObject;
    }

    private List<GameObject> GetAllAdjacentTiles()
    {
        var items = new List<GameObject>();
        foreach (var dir in adjacentDirections)
            items.Add(GetAdjacentTile(dir));

        return items;
    }

    private List<GameObject> FindMatch(Vector2 castDir)
    {
        var matchingTiles = new List<GameObject>();
        var hit = Physics2D.Raycast(transform.position, castDir);
        while (hit.collider != null && hit.collider.GetComponent<SpriteRenderer>().sprite == render.sprite)
        {
            matchingTiles.Add(hit.collider.gameObject);
            hit = Physics2D.Raycast(hit.collider.transform.position, castDir);
        }
        return matchingTiles;
    }

    private bool ClearMatch(params Vector2[] paths)
    {
        var matchingTiles = new List<GameObject>();
        foreach (var vector in paths)
            matchingTiles.AddRange(FindMatch(vector));

        if (matchingTiles.Count >= 2)
        {
            foreach (var tile in matchingTiles)
                tile.GetComponent<SpriteRenderer>().sprite = null;

            return true;
        }

        return false;
    }

    public void ClearAllMatches()
    {
        if (render.sprite == null)
            return;

        bool matchH = ClearMatch(Vector2.left, Vector2.right);
        bool matchV = ClearMatch(Vector2.up, Vector2.down);
        if (matchH || matchV)
        {
            render.sprite = null;
            SFXManager.instance.PlaySFX(Clip.Clear);
        }
    }

    public void SwapSprite(SpriteRenderer other)
    {
        if (render.sprite == other.sprite)
            return;

        (render.sprite, other.sprite) = (other.sprite, render.sprite);
        SFXManager.instance.PlaySFX(Clip.Swap);
    }
}