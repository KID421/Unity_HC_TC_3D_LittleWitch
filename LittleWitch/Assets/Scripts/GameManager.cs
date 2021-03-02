using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public void Replay()
    {
        SceneManager.LoadScene("關卡一");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
