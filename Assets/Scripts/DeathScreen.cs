using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathScreen : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreValueText;
    private GameObject _panel;

    // Start is called before the first frame update
    void Start()
    {
        _panel = gameObject.GetComponentInChildren<VerticalLayoutGroup>().gameObject;

        if (_panel)
        {
            _panel.SetActive(false);
        }
    }

    public void ShowDeathScreen(int score)
    {
        if (_panel)
        {
            scoreValueText.text = "Score: " + score;
            _panel.SetActive(true);
            Time.timeScale = 0;
        }
    }

    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void MainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }
}
