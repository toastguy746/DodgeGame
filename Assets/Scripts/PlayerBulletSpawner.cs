using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBulletSpawner : MonoBehaviour
{
    public GameObject bulletPrefab;
    public AudioClip ThrowSnow;
    private AudioSource audioSource; // AudioSource 변수 선언

    void Start()
    {
        // AudioSource 가져오기 또는 새로 추가
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 총알 생성
            GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);

            // 사운드 재생
            if (ThrowSnow != null) audioSource.PlayOneShot(ThrowSnow);
        }
    }
}
