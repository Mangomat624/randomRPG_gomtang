using System.Collections; // 코루틴 사용
using UnityEngine;
using System; // Action 사용
using UnityEngine.UI;

public class CharacterStats : MonoBehaviour
{
    public EquipmentDatabase equipmentDB;
    public static GachaManager Instance { get; private set; }
    public MonsterDatabase monsterDB;

    public enum PlayerType { Player, Enemy }

    [Header("Info")]
    public PlayerType playerType;
    public string unitName; // 유닛 이름 (예: "플레이어", "슬라임")
    public int currentHP;   // 현재 체력

    [Header("Attack Stats")]
    public int baseDamage = 5; // 기본 공격력 (기존 attackPower)
    public int maxHP = 100;       // 최대 체력
    public int baseAccuracy = 30; // 기본 적중
    public int weaponAccuracy = 0; // 무기 적중
    public int enhancementAccuracy = 0; // 강화 적중

    [Header("Defense Stats")]
    public int evasionRate = 1; // 회피율
    public int defensivePower = 0;

    [Header("UI")]
    public BattleUISystem uisysytem;
    public SpriteRenderer characterImage;
    public Sprite characterIdle;
    public Sprite characterAttack;
    public Sprite characterDied;

    [Header("Visual Effect")]
    public float squashDuration = 1f; // 찌그러지는 데 걸리는 시간

    [Header("Only Enemy")]
    public string enemyDesc; // 적 설명

    public Action OnDeathAnimationFinished;

    [Header("Animation Settings")]
    public float shakeDuration = 0.5f;   // 데미지 쉐이크 시간
    public float shakeMagnitude = 0.3f;  // 데미지 쉐이크 강도
    public float evadeDistance = 0.8f;   // 회피 시 이동 거리
    public float evadeDuration = 0.3f;   // 회피 애니메이션 총 시간

    private Vector3 originalPosition; // 캐릭터의 원래 위치
    private Coroutine currentAnimation; // 현재 재생 중인 애니메이션

    void Awake()
    {
        int currentStage = GameManager.Instance.currentStage;
        MonsterData monsterdata = monsterDB.GetMonstersByStage(currentStage);
        originalPosition = transform.position;

        if (playerType == PlayerType.Player)
        {
            LoadAllPartsStats_P();
        }
        else
        {
            LoadAllPartsStats_E();
        }

        IdleImage();

        currentHP = maxHP;
        uisysytem.SetMaxHealth(playerType, maxHP);
    }

    public void LoadAllPartsStats_P()
    {
        unitName = GameManager.Instance.playerName;
        foreach (EquipmentData.PartType part in System.Enum.GetValues(typeof(EquipmentData.PartType)))
        {
            EquipmentData.Grade currentPartGrade; // 이 부위의 현재 등급을 저장할 변수

            // 6. (핵심) 부위(part)에 맞는 GameManager의 등급 변수를 가져옵니다.
            switch (part)
            {
                case EquipmentData.PartType.Head:
                    currentPartGrade = GameManager.Instance.CurrentHeadGrade;
                    break;
                case EquipmentData.PartType.Torso:
                    currentPartGrade = GameManager.Instance.CurrentTorsoGrade;
                    break;
                case EquipmentData.PartType.Legs:
                    currentPartGrade = GameManager.Instance.CurrentLegsGrade;
                    break;
                case EquipmentData.PartType.Weapon:
                    currentPartGrade = GameManager.Instance.CurrentWeaponGrade;
                    break;
                default:
                    // 혹시 모를 예외 처리
                    currentPartGrade = EquipmentData.Grade.F;
                    break;
            }
            EquipmentData data = equipmentDB.GetData(part, currentPartGrade);

            if (data != null)
            {
                switch (part)
                {
                    case EquipmentData.PartType.Head:
                        maxHP = data.hp;
                        break;

                    case EquipmentData.PartType.Torso:
                        defensivePower = data.def;
                        break;

                    case EquipmentData.PartType.Legs:
                        evasionRate = data.eva;
                        break;

                    case EquipmentData.PartType.Weapon:
                        baseDamage = data.atk;
                        baseAccuracy = data.acc;
                        break;
                }
            }
        }



    }

    public void LoadAllPartsStats_E()
    {
        int currentStage = GameManager.Instance.currentStage;
        MonsterData monsterdata = monsterDB.GetMonstersByStage(currentStage);

        if (monsterdata != null)
        {
            maxHP = monsterdata.hp;
            baseDamage = monsterdata.atk;
            evasionRate = monsterdata.eva;
            unitName = monsterdata.monsterName;
            enemyDesc = monsterdata.des;
            characterIdle = monsterdata.idle;
            characterAttack = monsterdata.fight;
            characterDied = monsterdata.died;
        }
    }

    public void AttackImage()
    {
        if (playerType == PlayerType.Enemy)
        {
            characterImage.sprite = characterAttack;
        }
    }

    public void IdleImage()
    {
        if (playerType == PlayerType.Enemy)
        {
            characterImage.sprite = characterIdle;
        }
    }

    public bool TakeDamage(int damage)
    {
        currentHP -= damage;
        if (currentHP < 0) currentHP = 0; // HP가 음수가 되지 않도록

        // (변경) 애니메이션이 실행 중이라면 일단 멈춤
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            transform.position = originalPosition; // 즉시 원위치
        }

        // 체력이 0 이하로 떨어졌는지 확인
        if (currentHP <= 0)
        {
            Debug.Log(unitName + "이(가) 쓰러졌습니다.");

            uisysytem.SetHealth(playerType, currentHP, () =>
            {
                StartCoroutine(SquashEffect(() => OnDeathAnimationFinished?.Invoke()));
            });

            return true; // 쓰러졌음을 알림
        }
        else
        {
            // HP가 0보다 클 때:
            // 이전과 같이 콜백 없이 SetHealth만 호출합니다.
            currentAnimation = StartCoroutine(ShakeAnimation());
            uisysytem.SetHealth(playerType, currentHP);
            Debug.Log(unitName + "이(가) " + damage + "의 피해를 입었습니다. 남은 체력: " + currentHP);
            return false; // 아직 쓰러지지 않았음을 알림
        }
    }

    // 캐릭터가 찌그러지는 효과를 연출하는 코루틴
    private IEnumerator SquashEffect(Action onComplete)
    {
        if (playerType == PlayerType.Enemy)
        {
            int currentStage = GameManager.Instance.currentStage;

            MonsterData monsterdata = monsterDB.GetMonstersByStage(currentStage);

            characterImage.sprite = monsterdata.died;
        }

        yield return new WaitForSeconds(0.6f);

        Vector3 startScale = transform.localScale; // 현재 크기
        Vector3 endScale = new Vector3(startScale.x, 0, startScale.z); // 목표 크기 (Y축만 0)
        float timer = 0f;

        while (timer < squashDuration)
        {
            timer += Time.deltaTime;
            float t = timer / squashDuration;
            t = Mathf.Sin(t * Mathf.PI * 0.5f); // 부드러운 Ease-Out 효과 적용

            // Lerp를 사용해 시간에 따라 크기를 서서히 변경
            transform.localScale = Vector3.Lerp(startScale, endScale, t);

            yield return null;
        }

        // 정확한 목표 크기로 설정
        transform.localScale = endScale;

        onComplete?.Invoke();

        // 애니메이션이 끝난 후 캐릭터 오브젝트를 비활성화.
        gameObject.SetActive(false);
    }

    public void PlayEvadeAnimation()
    {
        // 다른 애니메이션이 실행 중이면 멈춤
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
        currentAnimation = StartCoroutine(EvadeAnimation());
    }

    // --- (추가) 데미지 (좌우 흔들기) 코루틴 ---
    private IEnumerator ShakeAnimation()
    {
        float timer = 0f;
        while (timer < shakeDuration)
        {
            float t = timer / shakeDuration;
            float currentMagnitude = shakeMagnitude * (1f - Mathf.Sin(t * Mathf.PI * 0.5f));
            // Sin 함수를 사용해 부드럽고 빠른 좌우 진동 생성
            float x = originalPosition.x + Mathf.Sin(timer * 80f) * currentMagnitude;
            transform.position = new Vector3(x, originalPosition.y, originalPosition.z);

            timer += Time.deltaTime;
            yield return null;
        }
        // 애니메이션이 끝나면 정확히 원래 위치로
        transform.position = originalPosition;
        currentAnimation = null; // 코루틴 종료
    }
    // --- 여기까지 ---

    // --- (추가) 회피 (왼쪽으로 갔다가 돌아오기) 코루틴 ---
    private IEnumerator EvadeAnimation()
    {
        float timer = 0f;
        float halfDuration = evadeDuration / 2;
        Vector3 evadePos = originalPosition - new Vector3(evadeDistance, 0, 0); // 왼쪽으로 이동할 위치

        // 1. 왼쪽으로 이동 (Ease-Out: 부드럽게 감속)
        while (timer < halfDuration)
        {
            float t = timer / halfDuration;
            t = Mathf.Sin(t * Mathf.PI * 0.5f); // Ease-Out
            transform.position = Vector3.Lerp(originalPosition, evadePos, t);
            timer += Time.deltaTime;
            yield return null;
        }

        // 2. 다시 오른쪽(원위치)으로 이동 (Ease-In: 부드럽게 가속)
        timer = 0f; // 타이머 리셋
        while (timer < halfDuration)
        {
            float t = timer / halfDuration;
            t = 1f - Mathf.Cos(t * Mathf.PI * 0.5f); // Ease-In
            transform.position = Vector3.Lerp(evadePos, originalPosition, t);
            timer += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition; // 정확히 원위치
        currentAnimation = null; // 코루틴 종료
    }
}
