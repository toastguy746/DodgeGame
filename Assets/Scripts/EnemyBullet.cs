using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
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

        if (other.tag == "Wall")
        {
            Destroy(gameObject);
        }

        else if(other.tag == "Player")
        {
            PlayerController playerController = other.GetComponent<PlayerController>();
            if(playerController != null)
            {
                playerController.TakeDamagePlayer(10);
            }
        }

    }

}
