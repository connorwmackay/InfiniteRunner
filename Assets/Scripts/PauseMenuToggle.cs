using UnityEngine;
using UnityEngine.SceneManagement;

namespace DefaultNamespace
{
    public class PauseMenuToggle : MenuToggle
    {
        protected override void OnMenuOpened()
        {
            Time.timeScale = 0;
        }
        
        protected override void OnMenuClosed()
        {
            Time.timeScale = 1;
        }

        public void OnMenuMenuBtnClicked()
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(0);
        }
    }
}