using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Obstacle : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            GameObject scoreManagerObject = GameObject.FindGameObjectWithTag("ScoreManager");
            ScoreManager scoreManager = scoreManagerObject.GetComponent<ScoreManager>();
            int score = scoreManager.GetScore();
            DeathScreen deathScreen = GameObject.FindGameObjectWithTag("DeathMenu").GetComponent<DeathScreen>();
            deathScreen.ShowDeathScreen(score);
        }
    }
}
