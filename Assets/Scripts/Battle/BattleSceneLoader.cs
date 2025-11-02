using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Unity.VisualScripting; // 코루틴 사용
using UnityEngine.UI; // Image를 사용하기 위해 이 줄을 추가합니다.

public class BattleSceneLoader : MonoBehaviour
{
    public Material transitionMaterial;
    public float transitionSpeed = 1f; // 전환 속도

    public Texture closingPatternTexture;

    public Image flashImage;
    public float fadeInTime = 0.22f;  // (13프레임)
    public float fadeOutTime = 0.22f; // (13프레임)
    public int flashCount = 2;          // 깜빡일 횟수


    void Start()
    {
        transitionMaterial.SetFloat("_Cutoff", 0f);
    }

    public void LoadBattleScene()
    {
        // "Battle" 씬을 로드하는 코루틴을 시작합니다.
        StartCoroutine(LoadSceneWithTransition("전투"));
    }
    private IEnumerator LoadSceneWithTransition(string sceneName)
    {
        if (flashImage != null)
        {
            // 발광 이펙트 코루틴을 실행하고 끝날 때까지 기다립니다.
            yield return StartCoroutine(FlashScreenFade(flashCount));
        }

        if (closingPatternTexture != null)
        {
            transitionMaterial.SetTexture("_TransitionTex", closingPatternTexture);
        }

        // 2. Cutoff 값을 0으로 즉시 설정 (열린 상태에서 시작)
        transitionMaterial.SetFloat("_Cutoff", 0f);

        float t = 0;

        // 4. Cutoff 값을 0에서 1로 (화면 닫기)
        while (t < 1)
        {
            t += Time.deltaTime * transitionSpeed;
            // 셰이더의 _Cutoff 값을 0에서 1로 변경
            transitionMaterial.SetFloat("_Cutoff", t);
            yield return null;
        }

        // 5. 애니메이션이 끝나면 씬 로드
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FlashScreenFade(int count)
    {
        // 1. 시작하기 전에 색상 변수를 준비하고, 알파값 0, 오브젝트 활성화
        Color flashColor = flashImage.color;
        flashColor.a = 0f;
        flashImage.color = flashColor;
        flashImage.gameObject.SetActive(true); // 이펙트가 보이도록 오브젝트 켬

        for (int i = 0; i < count; i++)
        {
            // --- 페이드 인 (점점 밝아짐) ---
            float timer = 0f;
            while (timer < fadeInTime)
            {
                timer += Time.deltaTime;
                // Lerp를 사용해 알파값을 0에서 1로 부드럽게 변경
                flashColor.a = Mathf.Lerp(0f, 1f, timer / fadeInTime);
                flashImage.color = flashColor;
                yield return null;
            }
            // 페이드 인 완료 (알파 1로 고정)
            flashColor.a = 1f;
            flashImage.color = flashColor;

            // --- 페이드 아웃 (점점 어두워짐) ---
            timer = 0f; // 타이머 초기화
            while (timer < fadeOutTime)
            {
                timer += Time.deltaTime;
                // Lerp를 사용해 알파값을 1에서 0으로 부드럽게 변경
                flashColor.a = Mathf.Lerp(1f, 0f, timer / fadeOutTime);
                flashImage.color = flashColor;
                yield return null;
            }
            // 페이드 아웃 완료 (알파 0으로 고정)
            flashColor.a = 0f;
            flashImage.color = flashColor;
        }

        // 3. 모든 깜빡임이 끝나면 오브젝트를 다시 끔
        flashImage.gameObject.SetActive(false);
    }
}
