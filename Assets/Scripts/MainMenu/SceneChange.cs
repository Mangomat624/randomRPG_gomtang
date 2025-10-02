using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    public SceneAsset targetScene;


    public void LoadTargetScene()
    {
        if (targetScene != null)
        {
            // SceneAsset의 이름(파일 이름)을 사용해 씬을 로드합니다.
            SceneManager.LoadScene(targetScene.name);
        }
    }
}
