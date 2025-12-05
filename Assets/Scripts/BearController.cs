using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events; // ⭐ 부활 시 카메라 전환 등 이벤트용

public class BearController : MonoBehaviour
{
    [Header("State Flags")]
    private Transform target;
    private bool dying = false;          // 최종 사망 처리 중 여부
    private bool frenzy = false;
    private bool isAttacking = false;
    public Transform firePoint;
    private bool firstDeathHandled = false;
    private bool isReviving = false;

    // 첫 번째 죽음 시, 현재 패턴이 끝난 뒤에 부활시키기 위한 플래그
    private bool pendingFirstRevive = false;

    // 두 번째(최종) 죽음 예약 플래그 (현재 공격 끝난 뒤 처리)
    private bool pendingFinalDeath = false;

    // ✅ 부활 이후 일정 시간 동안 패턴이 다시 시작되지 않도록 잠그는 타이머
    private float patternResumeTime = 0f;

    [Header("Movement & Attack Settings")]
    public float moveSpeed = 2f;
    public Collider attack12;              // 근접 판정용 콜라이더
    private bool isShootingPattern3 = false;
    private Coroutine pattern3Routine;     // 패턴3 코루틴 핸들

    [Header("HP & Audio")]
    public float enemyMaxHp = 1000f;
    public float enemyHp;
    private AudioSource audioSource;
    public AudioClip bossHitSound;

    [Header("Components")]
    public Animator animator;
    [SerializeField] private Slider _hpBar;

    [Header("Bullet Prefabs")]
    public GameObject bigbulletPrefab;
    public GameObject smallbulletPrefab;

    // ⭐ 카메라 레퍼런스
    [Header("Cameras")]
    public Camera mainCamera;     // 평소 사용하는 카메라
    public Camera reviveCamera;   // 곰 자식으로 둔 연출용 카메라

    // ⭐ 부활 동안 카메라/연출 전환용 이벤트 (선택)
    [Header("Revive Events (for Camera, VFX etc.)")]
    public UnityEvent OnFirstReviveStart;
    public UnityEvent OnFirstReviveEnd;

    // ⭐ 부활 중 플레이어 위치 고정 관련
    [Header("Revive Lock Settings")]
    // 고정할 위치 (요청한 좌표)
    [SerializeField]
    private Vector3 reviveLockPosition = new Vector3(0.230000108f, 1.09999943f, -25.2199917f);

    private bool lockPlayerDuringRevive = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        enemyHp = enemyMaxHp;

        var player = FindObjectOfType<PlayerController>();
        if (player != null)
            target = player.transform;

        if (animator == null)
            animator = GetComponent<Animator>();

        if (attack12 != null)
            attack12.enabled = true;

        if (_hpBar != null)
        {
            _hpBar.maxValue = enemyMaxHp;
            _hpBar.value = enemyHp;
        }

        // 카메라 초기 상태: 메인 ON, 부활 카메라 OFF
        InitCameras();

        StartCoroutine(StartPatternRoutine());
    }

    void Update()
    {
        // ⭐ 부활 중에는 플레이어를 지정된 위치에 고정
        if (isReviving && lockPlayerDuringRevive && target != null)
        {
            target.position = reviveLockPosition;
        }

        if (!dying && !isReviving && target != null)
        {
            // y만 맞추고 회전
            Vector3 lookPos = target.position;
            lookPos.y = transform.position.y;
            transform.LookAt(lookPos);
        }
    }

    // ------------------------------------
    // 카메라 제어
    // ------------------------------------
    private void InitCameras()
    {
        if (mainCamera != null)
            mainCamera.enabled = true;

        if (reviveCamera != null)
            reviveCamera.enabled = false;
    }

    private void SwitchToReviveCamera()
    {
        if (reviveCamera != null)
            reviveCamera.enabled = true;

        if (mainCamera != null)
            mainCamera.enabled = false;
    }

    private void SwitchToMainCamera()
    {
        if (reviveCamera != null)
            reviveCamera.enabled = false;

        if (mainCamera != null)
            mainCamera.enabled = true;
    }

    // ------------------------------------
    // 데미지 처리
    // ------------------------------------
    public void TakeDamageEnemy(float damage)
    {
        // 부활 중이거나, 이미 죽음 예약/처리 중이면 무시
        if (isReviving) return;
        if (enemyHp <= 0f && (pendingFirstRevive || pendingFinalDeath || dying))
            return;

        enemyHp -= damage;

        if (enemyHp < 0f)
            enemyHp = 0f;

        if (_hpBar != null)
            _hpBar.value = enemyHp;

        if (bossHitSound != null && audioSource != null)
            audioSource.PlayOneShot(bossHitSound);

        if (!isAttacking && animator != null)
            animator.SetTrigger("Get Hit Front");

        if (!frenzy && enemyHp <= enemyMaxHp * 0.5f)
            frenzy = true;

        if (enemyHp <= 0f)
        {
            // 첫 번째 죽음 → "부활 예약"만 걸어두고, 현재 패턴 끝난 뒤 FirstRevive에서 처리
            if (!firstDeathHandled)
            {
                if (!pendingFirstRevive)
                    pendingFirstRevive = true;
                return;
            }

            // 두 번째(최종) 죽음 → "최종 죽음 예약"
            // 현재 진행 중인 공격이 끝나고 AttackPatternRoutine 루프 상단에서 처리
            if (!pendingFinalDeath)
                pendingFinalDeath = true;
        }
    }

    // ------------------------------------
    // 부활 처리 (첫 번째 죽음 전용)
    // ------------------------------------
    private IEnumerator FirstRevive()
    {
        firstDeathHandled = true;
        isReviving = true;

        // ⭐ 플레이어 위치 고정 시작
        lockPlayerDuringRevive = true;

        // ⭐ 부활 시작: 카메라 전환 + 이벤트 호출
        SwitchToReviveCamera();
        OnFirstReviveStart?.Invoke();

        if (animator != null)
            animator.SetTrigger("Buff");

        float targetHpValue = 500f;
        float duration = 1f;       // 이 1초 동안 연출 카메라 사용
        float startHp = enemyHp;   // 보통 0
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            enemyHp = Mathf.Lerp(startHp, targetHpValue, t);

            if (_hpBar != null)
                _hpBar.value = enemyHp;

            // 부활 동안 계속 총알 삭제
            foreach (var bullet in GameObject.FindGameObjectsWithTag("BigBullet"))
                Destroy(bullet);
            foreach (var bullet in GameObject.FindGameObjectsWithTag("SmallBullet"))
                Destroy(bullet);

            // 플레이어 위치를 한 번 더 보정 (안전용)
            if (lockPlayerDuringRevive && target != null)
                target.position = reviveLockPosition;

            yield return null;
        }

        enemyHp = targetHpValue;
        if (_hpBar != null)
            _hpBar.value = enemyHp;

        // 부활 끝
        isReviving = false;

        // ⭐ 플레이어 위치 고정 해제 (위치는 그대로)
        lockPlayerDuringRevive = false;

        // ⭐ 부활 끝: 카메라 원상복구 + 이벤트 호출
        SwitchToMainCamera();
        OnFirstReviveEnd?.Invoke();

        // ✅ 지금 시점 기준으로 2초 동안은 패턴이 아예 안 돌아가도록 잠금
        patternResumeTime = Time.time + 2f;
    }

    // ------------------------------------
    // 최종 죽음 처리 진입 (현재 패턴 끝난 뒤 호출)
    // ------------------------------------
    private void BeginFinalDeath()
    {
        dying = true;
        isAttacking = false;

        // 공격/패턴 관련 전부 중지
        StopPattern3();

        if (attack12 != null)
            attack12.enabled = false;

        if (animator != null)
            animator.SetBool("Death", true);

        // 실제 제거는 약간 대기 후
        StartCoroutine(DieAfterSound());
    }

    // ------------------------------------
    // 패턴 시작 / 루프
    // ------------------------------------
    private IEnumerator StartPatternRoutine()
    {
        yield return new WaitForSeconds(2f);   // 시작 전 준비 시간
        yield return StartCoroutine(AttackPatternRoutine());
    }

    private IEnumerator AttackPatternRoutine()
    {
        while (true)
        {
            // 1) 부활 예약이 걸려 있으면, 다른 패턴보다 우선해서 부활 먼저
            if (pendingFirstRevive)
            {
                pendingFirstRevive = false;
                yield return StartCoroutine(FirstRevive());
                continue;
            }

            // 1.5) 최종 죽음 예약이 걸려 있으면,
            //      "현재 공격이 끝나고 루프 상단에 도달한 시점"에서 죽음 함수 진입
            if (pendingFinalDeath)
            {
                pendingFinalDeath = false;
                BeginFinalDeath();
                yield break;       // 패턴 루프 완전히 종료
            }

            // 2) 부활 중이거나, 부활 후 2초 쿨타임이 아직 남아있으면 패턴 시작 금지
            if (isReviving || Time.time < patternResumeTime)
            {
                yield return null;
                continue;
            }

            // -------- Attack1 --------
            isAttacking = true;
            if (animator != null)
                animator.SetTrigger("Attack1");

            DealAttackDamage(bigbulletPrefab);
            Debug.Log("어택1");

            yield return new WaitForSeconds(2f);
            isAttacking = false;

            if (pendingFirstRevive || pendingFinalDeath || isReviving || Time.time < patternResumeTime)
                continue;

            // -------- Attack2 --------
            isAttacking = true;
            if (animator != null)
                animator.SetTrigger("Attack2");

            DealAttackDamage(bigbulletPrefab);
            Debug.Log("어택2");

            yield return new WaitForSeconds(2f);
            isAttacking = false;

            if (pendingFirstRevive || pendingFinalDeath || isReviving || Time.time < patternResumeTime)
                continue;

            // -------- Attack3 (탄막) --------
            isAttacking = true;
            if (animator != null)
                animator.SetTrigger("Buff");

            // 패턴3 시작: 내부에서 약 2초 동안만 탄막 발사
            StartPattern3();
            Debug.Log("어택3");

            yield return new WaitForSeconds(2f);

            StopPattern3();
            isAttacking = false;
        }
    }

    // ------------------------------------
    // 죽음 처리
    // ------------------------------------
    private IEnumerator DieAfterSound()
    {
        // ⭐ 두 번째 죽음 시 3.5초 대기 후 삭제
        yield return new WaitForSeconds(3.5f);
        EnemyDie();
    }

    public void EnemyDie()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.score += 100;
            gameManager.enemyCount--;
        }
        gameObject.SetActive(false);
    }

    // ------------------------------------
    // 공격 1/2의 탄 + 근접 데미지
    // ------------------------------------
    public void DealAttackDamage(GameObject bulletPrefab)
    {
        if (attack12 == null || bulletPrefab == null || target == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Vector3 forward = (target.position - spawnPos).normalized;
        if (forward == Vector3.zero) forward = transform.forward;

        // 중앙 + 좌/우 30도 3발
        SpawnBullet(bulletPrefab, spawnPos, forward);
        SpawnBullet(bulletPrefab, spawnPos, Quaternion.Euler(0, -30f, 0) * forward);
        SpawnBullet(bulletPrefab, spawnPos, Quaternion.Euler(0, 30f, 0) * forward);

        // 근접 박스 데미지
        Collider[] hits = Physics.OverlapBox(
            attack12.bounds.center,
            attack12.bounds.extents,
            attack12.transform.rotation
        );

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerController pc = hit.GetComponent<PlayerController>();
                if (pc != null)
                    pc.TakeDamagePlayer(10);
            }
        }
    }

    private void SpawnBullet(GameObject prefab, Vector3 pos, Vector3 dir)
    {
        if (prefab == null) return;
        if (dir == Vector3.zero) dir = transform.forward;

        Instantiate(prefab, pos, Quaternion.LookRotation(dir));
    }

    // ------------------------------------
    // Attack3 총알 패턴 (2초 + 시작 위치 스냅샷)
    // ------------------------------------
    public void StartPattern3()
    {
        if (!isShootingPattern3)
        {
            isShootingPattern3 = true;
            pattern3Routine = StartCoroutine(Pattern3());
        }
    }

    public void StopPattern3()
    {
        isShootingPattern3 = false;

        if (pattern3Routine != null)
        {
            StopCoroutine(pattern3Routine);
            pattern3Routine = null;
        }
    }

    private IEnumerator Pattern3()
    {
        if (smallbulletPrefab == null || target == null) yield break;

        int bulletsPerRow = 5;       // 한 줄에 발사할 총알 수
        float angleStep = 20f;       // 총알 사이 각도
        float rowDelay = 0.5f;       // 한 줄 발사 후 딜레이
        float maxDuration = 2f;      // 패턴3 전체 지속 시간 ≒ 2초

        float elapsed = 0f;

        // ✅ 패턴 시작 시점의 타겟 위치 스냅샷
        Vector3 targetPosAtStart = target.position;

        while (isShootingPattern3 && elapsed < maxDuration)
        {
            if (target == null) yield break;

            // 발사 위치는 현재 firePoint에서,
            // 방향은 "처음 스냅샷한 타겟 위치" 기준으로 고정
            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
            Vector3 forward = (targetPosAtStart - spawnPos).normalized;
            if (forward == Vector3.zero) forward = transform.forward;

            float startAngle = -angleStep * (bulletsPerRow - 1) / 2f;
            for (int j = 0; j < bulletsPerRow; j++)
            {
                float angleY = startAngle + angleStep * j;
                Vector3 dir = Quaternion.Euler(0, angleY, 0) * forward;
                SpawnBullet(smallbulletPrefab, spawnPos, dir);
            }

            yield return new WaitForSeconds(rowDelay);
            elapsed += rowDelay;
        }

        isShootingPattern3 = false;
        pattern3Routine = null;
    }
}
