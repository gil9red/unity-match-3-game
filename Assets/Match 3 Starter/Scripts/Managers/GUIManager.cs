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

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour
{
    public static GUIManager Instance;

    public GameObject gameOverPanel;
    public Text yourScoreTxt;
    public Text highScoreTxt;

    public Text scoreTxt;
    public Text moveCounterTxt;

    private int _score;

    public int Score
    {
        get => _score;
        set
        {
            _score = value;
            scoreTxt.text = _score.ToString();
        }
    }

    private int _moveCounter;

    public int MoveCounter
    {
        get => _moveCounter;
        set
        {
            _moveCounter = value;
            if (_moveCounter <= 0)
            {
                _moveCounter = 0;
                StartCoroutine(WaitForShifting());
            }

            moveCounterTxt.text = _moveCounter.ToString();
        }
    }

    void Awake()
    {
        Instance = GetComponent<GUIManager>();
        MoveCounter = GameManager.Instance.MoveCounter;
    }

    // Show the game over panel
    public void GameOver()
    {
        GameManager.Instance.GameOver = true;

        gameOverPanel.SetActive(true);

        if (Score > PlayerPrefs.GetInt("HighScore"))
        {
            PlayerPrefs.SetInt("HighScore", Score);
            highScoreTxt.text = "New Best: " + PlayerPrefs.GetInt("HighScore").ToString();
        }
        else
        {
            highScoreTxt.text = "Best: " + PlayerPrefs.GetInt("HighScore").ToString();
        }

        yourScoreTxt.text = Score.ToString();
    }

    private IEnumerator WaitForShifting()
    {
        yield return new WaitForSeconds(.25f);
        yield return new WaitUntil(() => !BoardManager.Instance.IsShifting);
        GameOver();
    }
}
