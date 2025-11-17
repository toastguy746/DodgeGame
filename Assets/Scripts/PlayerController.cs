using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody playerRigidBody;
    public float speed = 8f; //1000
    // Start is called before the first frame update
    void Start()
    {
        playerRigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        float xSpeed = x * speed;
        float zSpeed = z * speed;

        Vector3 dir = new Vector3(xSpeed, 0f, zSpeed);

        playerRigidBody.velocity = dir;

        if (Input.GetKey(KeyCode.W))
        {
            playerRigidBody.AddForce(0f, 0f, speed);
        }
        if (Input.GetKey(KeyCode.S))
        {
            playerRigidBody.AddForce(0f, 0f, -speed);
        }

        if (Input.GetKey(KeyCode.D))
        {
            playerRigidBody.AddForce(speed, 0f, 0f);
        }
        if (Input.GetKey(KeyCode.A))
        {
            playerRigidBody.AddForce(-speed, 0f, 0f);
        }


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
