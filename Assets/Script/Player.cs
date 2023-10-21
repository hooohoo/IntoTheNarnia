using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    float moveSpeed;

    void Start()
    {
        moveSpeed = 5f;
    }

    void Update()
    {
        if(Input.GetKey(KeyCode.UpArrow))
        {
            float z = transform.position.z;
            z += Time.deltaTime * moveSpeed;
            transform.position = new Vector3(transform.position.x, transform.position.y, z);
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            float z = transform.position.z;
            z -= Time.deltaTime * moveSpeed;
            transform.position = new Vector3(transform.position.x, transform.position.y, z);
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            float x = transform.position.x;
            x += Time.deltaTime * moveSpeed;
            transform.position = new Vector3(x, transform.position.y, transform.position.z);
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            float x = transform.position.x;
            x -= Time.deltaTime * moveSpeed;
            transform.position = new Vector3(x, transform.position.y, transform.position.z);
        }

    }
}
