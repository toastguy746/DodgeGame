using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    private Rigidbody bulletRigidbody;
    public float speed = 8f;
    // Start is called before the first frame update
    void Start()
    {
        bulletRigidbody = GetComponent<Rigidbody>();
        //오브젝트내에서 트랜스폼은 직접 참조 가능
        bulletRigidbody.velocity = transform.forward * speed;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.tag == "Enemy")
        {
            Destroy(other.gameObject);
            Destroy(gameObject);
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.enemyCount--;
                Debug.Log("적 사망! 남은 적 수 : "+gameManager.enemyCount);
            }
        }

        else if (other.tag == "Wall")
        {
            Destroy(gameObject);
        }

    }

}
