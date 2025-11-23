using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float spawnRateMin = 2f;
    public float spawnRateMax = 5f;

    private Transform target;
    private float spawnRate;
    private float timeAfterSpawn = 0;

    public float enemyMaxHp;
    public float enemyHp;
    public bool enemyDie;
    public AudioClip enemyHitSound;
    private AudioSource audioSource;

    public GameObject enemyDieIce;
    // Start is called before thfe first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (enemyHp > enemyMaxHp)
        {
            enemyHp = enemyMaxHp;
        }

        spawnRate = Random.Range(spawnRateMin, spawnRateMax);
        target = FindObjectOfType<PlayerController>().transform;
    }

    // Update is called once per frame
    void Update()
    {
        timeAfterSpawn += Time.deltaTime;
        if (timeAfterSpawn > spawnRateMin)
        {
            timeAfterSpawn = 0;
            GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
            bullet.transform.LookAt(target);
            spawnRate = Random.Range(spawnRateMin, spawnRateMax);
        }
    }

    public void TakeDamageEnemy(float damage)
    {
        enemyHp -= damage;
        Debug.Log("적 체력 감소! 적 현재 체력 : " + enemyHp);
        if (enemyHp <= 0)
        {
            EnemyDie();
        }
        if (enemyHitSound != null && !enemyDie) audioSource.PlayOneShot(enemyHitSound);
    }

    public void EnemyDie()
    {
        enemyDie = true;
        enemyDieIce = Instantiate(enemyDieIce, transform.position, transform.rotation);
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.score += 2;
            gameManager.enemyCount--;
            Debug.Log("적 사망! 남은 적 수 : " + gameManager.enemyCount);
        }
        gameObject.SetActive(false);
    }
}
