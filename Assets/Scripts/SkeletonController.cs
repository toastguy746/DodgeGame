using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SkeletonController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    private Transform player;
    private bool isPlayerInRange = false;

    [Header("Combat / HP Settings")]
    public float enemyMaxHp = 200;
    public float enemyHp;
    private bool dying = false;

    [Header("Audio Settings")]
    public AudioClip enemyHitSound;
    private AudioSource audioSource;

    [Header("Death Settings")]
    public GameObject enemyDieIce;

    [Header("UI Settings")]
    [SerializeField] private Slider hpBar;

    [Header("Misc Settings")]
    public Vector3 offset = new Vector3(0, 2f, 0);
    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        enemyHp = enemyMaxHp;

        // HP바 초기값 세팅
        if (hpBar != null)
        {
            hpBar.maxValue = enemyMaxHp;
            hpBar.value = enemyHp;
        }
        // 씬에서 PlayerController를 찾아 참조
        PlayerController pc = FindObjectOfType<PlayerController>();
        if (pc != null)
            player = pc.transform;
    }

    void Update()
    {
        if (player == null) return;

        // 항상 플레이어 바라보기
        transform.LookAt(player);

        // 플레이어와의 충돌 체크 후 이동
        if (!isPlayerInRange)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }

    void LateUpdate()
    {
        if (hpBar != null)
        {
            // 플레이어 위치 + 오프셋
            hpBar.transform.position = transform.position + offset;

            // 카메라를 바라보게
            if (Camera.main != null)
            {
                hpBar.transform.LookAt(Camera.main.transform);
            }
        }
    }

    public void TakeDamageEnemy(float damage)
    {
        if (dying) return;

        enemyHp -= damage;
        Debug.Log("적 체력 감소! 적 현재 체력 : " + enemyHp);

        if (hpBar != null)
            hpBar.value = enemyHp;

        if (enemyHitSound != null) audioSource.PlayOneShot(enemyHitSound);

        if (enemyHp <= 0)
        {
            dying = true;
            StartCoroutine(DieAfterSound());
            return;
        }

    }

    private IEnumerator DieAfterSound()
    {
        // 죽기 전에 HIT 사운드를 끝까지 재생할 시간 확보
        yield return new WaitForSeconds(0.5f);
        EnemyDie();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            isPlayerInRange = true;

            PlayerController pc = collision.collider.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.TakeDamagePlayer(5);
            }
        }
    }

    public void EnemyDie()
    {
        enemyDieIce = Instantiate(enemyDieIce, transform.position, transform.rotation);
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.score += 2;
            gameManager.enemyCount--;
            Debug.Log("적 사망! 남은 적 수 : " + gameManager.enemyCount);
        }

        if (hpBar != null)
            Destroy(hpBar.gameObject);

        gameObject.SetActive(false);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }
}
