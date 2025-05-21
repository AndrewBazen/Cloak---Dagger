using UnityEngine;
using UnityEngine.SceneManagement;

namespace Start.Scripts
{
    public class MainMenu : MonoBehaviour
    {
        public void NewGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        public void QuitGame()
        {
            Debug.Log("Quit");
            Application.Quit();
        }
    }
}
