using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public  abstract class MenuToggle : MonoBehaviour
{
    [SerializeField] private KeyCode toggleKeyCode;
    private GameObject pauseMenuPanel;

    protected abstract void OnMenuOpened();

    protected abstract void OnMenuClosed();

    void Start()
    {
        pauseMenuPanel = gameObject.GetComponentInChildren<VerticalLayoutGroup>().gameObject;
        pauseMenuPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKeyCode))
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(!pauseMenuPanel.activeSelf);

            if (pauseMenuPanel.activeSelf)
            {
                OnMenuOpened();
            }
            else
            {
                OnMenuClosed();
            }
        }
    }
}
