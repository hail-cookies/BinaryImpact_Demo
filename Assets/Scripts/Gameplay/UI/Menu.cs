using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void Load(string scene)
    {
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }

    public void Toggle()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    private void OnEnable()
    {
        Time.timeScale = 0;
        Player.active = false;
    }

    private void OnDisable()
    {
        Time.timeScale = 1;
        Player.active = true;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1;
        Player.active = true;
    }
}
