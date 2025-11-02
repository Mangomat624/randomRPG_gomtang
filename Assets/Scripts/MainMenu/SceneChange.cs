using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    public string targetSceneName;
    public void LoadTargetScene()
    {
            SceneManager.LoadScene(targetSceneName);
    }
}
