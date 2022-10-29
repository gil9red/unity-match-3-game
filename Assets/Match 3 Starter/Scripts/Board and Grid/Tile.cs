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
    private static Color s_selectedColor = new Color(.5f, .5f, .5f, 1.0f);
    private static Color s_defaultColor;

    private static Tile s_previousSelected = null;

    private SpriteRenderer _render;
    private bool _isSelected = false;

    private Vector2[] _adjacentDirections = new Vector2[] {
        Vector2.up, Vector2.down, Vector2.left, Vector2.right
    };

    void Awake()
    {
        _render = GetComponent<SpriteRenderer>();
        s_defaultColor = _render.color;
    }

    private void Select()
    {
        _isSelected = true;
        _render.color = s_selectedColor;
        s_previousSelected = gameObject.GetComponent<Tile>();
        SFXManager.instance.PlaySFX(Clip.Select);
    }

    private void Deselect()
    {
        _isSelected = false;
        _render.color = s_defaultColor;
        s_previousSelected = null;
    }

    void OnMouseDown()
    {
        if (_render.sprite == null || BoardManager.Instance.IsShifting)
            return;

        if (_isSelected)
        {
            Deselect();
            return;
        }

        if (s_previousSelected == null)
        {
            Select();
            return;
        }

        // Поиск соседней фишки
        if (GetAllAdjacentTiles().Contains(s_previousSelected.gameObject))
        {
            SwapSprite(s_previousSelected._render);
            s_previousSelected.ClearAllMatches();
            s_previousSelected.Deselect();
            ClearAllMatches();
        }
        else
        {
            s_previousSelected.Deselect();
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
        foreach (var dir in _adjacentDirections)
            items.Add(GetAdjacentTile(dir));

        return items;
    }

    private List<GameObject> FindMatch(Vector2 castDir)
    {
        var matchingTiles = new List<GameObject>();
        var hit = Physics2D.Raycast(transform.position, castDir);
        while (hit.collider?.GetComponent<SpriteRenderer>().sprite == _render.sprite)
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
        {
            matchingTiles.AddRange(FindMatch(vector));
        }

        if (matchingTiles.Count >= 2)
        {
            foreach (var tile in matchingTiles)
            {
                tile.GetComponent<SpriteRenderer>().sprite = null;
            }

            return true;
        }

        return false;
    }

    public void ClearAllMatches()
    {
        if (_render.sprite == null)
            return;

        bool matchH = ClearMatch(Vector2.left, Vector2.right);
        bool matchV = ClearMatch(Vector2.up, Vector2.down);
        if (matchH || matchV)
        {
            _render.sprite = null;

            StopCoroutine(BoardManager.Instance.FindNullTiles());
            StartCoroutine(BoardManager.Instance.FindNullTiles());

            SFXManager.instance.PlaySFX(Clip.Clear);
        }
    }

    public void SwapSprite(SpriteRenderer other)
    {
        if (_render.sprite == other.sprite)
            return;

        (_render.sprite, other.sprite) = (other.sprite, _render.sprite);
        SFXManager.instance.PlaySFX(Clip.Swap);
    }
}