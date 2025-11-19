using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody playerRigidBody;
    public float speed = 8f; //1000
    public float playerRotationSpeed = 1f;
    public float playerMaxHp = 100;
    public float playerHp;
    private float damage;

    // Start is called before the first frame update
    void Start()
    {
        playerRigidBody = GetComponent<Rigidbody>();
        playerHp = playerMaxHp;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerHp > playerMaxHp)
        {
            playerHp = playerMaxHp;
        }

        if (playerHp <= 0)
        {
            Die();
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        float xSpeed = x * speed;
        float zSpeed = z * speed;

        Vector3 dir = new Vector3(xSpeed, 0f, zSpeed);

        playerRigidBody.velocity = dir;

        if (Input.GetKey(KeyCode.W))
        {
            playerRigidBody.AddForce(transform.forward * speed);
        }
        if (Input.GetKey(KeyCode.S))
        {
            playerRigidBody.AddForce(transform.forward * -speed);
        }

        if (Input.GetKey(KeyCode.D))
        {
            playerRigidBody.AddForce(transform.right * speed);
        }
        if (Input.GetKey(KeyCode.A))
        {
            playerRigidBody.AddForce(-transform.right * speed);
        }

        if(Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Rotate(0f, -playerRotationSpeed * Time.deltaTime, 0f);
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Rotate(0f, playerRotationSpeed * Time.deltaTime, 0f);
        }

    }
     public void TakeDamagePlayer(float damage)
     {
        playerHp -= damage;
        Debug.Log("체력 닳음! 현재 체력 : " + playerHp);
     }

     public void Die()
     {
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.EndGame();
        }
        gameObject.SetActive(false);
        
     }
}
