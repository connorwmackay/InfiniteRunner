using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private int score;
    private Vector3 initialPlayerPosition;
    private TMP_Text scoreText;

    // Start is called before the first frame update
    void Start()
    {
        score = 0;
        initialPlayerPosition = GameObject.FindGameObjectWithTag("Player").transform.position;
        scoreText = GameObject.FindGameObjectWithTag("Score").GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 currentPlayerPosition = GameObject.FindGameObjectWithTag("Player").transform.position;
        score = (int)Math.Floor(Vector3.Distance(initialPlayerPosition, currentPlayerPosition));

        if (scoreText != null)
        {
            scoreText.text = String.Concat(score, "m");
        }
    }

    public int GetScore()
    {
        return score;
    }
}
