using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectMenu : MonoBehaviour
{
    public void Level1()
    {
        SceneManager.LoadSceneAsync(2);
    }
    public void Boss1()
    {
        SceneManager.LoadSceneAsync(3);
    }
    public void Level2()
    {
        SceneManager.LoadSceneAsync(4);
    }
    public void Boss2()
    {
        SceneManager.LoadSceneAsync(5);
    }

    public void BackButton()
    {
        SceneManager.LoadSceneAsync(0);
    }
}
