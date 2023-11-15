using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    //public GameObject _character;
    float moveSpeed;
    float rotateSpeed;
    Vector3 tmpDir;

    void Start()
    {
        moveSpeed = 5f;
        rotateSpeed = 10f;
    }

    void Update()
    {
        if(Input.GetKey(KeyCode.UpArrow))
        {
            tmpDir = transform.forward;
            float z = transform.position.z;
            z += Time.deltaTime * moveSpeed;
            transform.position = new Vector3(transform.position.x, transform.position.y, z);
            // 임시 회전
            tmpDir = Vector3.RotateTowards(transform.forward, tmpDir, Time.deltaTime * rotateSpeed, 0);
            transform.rotation = Quaternion.LookRotation(tmpDir.normalized);
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            tmpDir = -(transform.forward);
            float z = transform.position.z;
            z -= Time.deltaTime * moveSpeed;
            transform.position = new Vector3(transform.position.x, transform.position.y, z);
            // 임시 회전
            tmpDir = Vector3.RotateTowards(transform.forward, tmpDir, Time.deltaTime * rotateSpeed, 0);
            transform.rotation = Quaternion.LookRotation(tmpDir.normalized);
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            tmpDir = transform.right;
            float x = transform.position.x;
            x += Time.deltaTime * moveSpeed;
            transform.position = new Vector3(x, transform.position.y, transform.position.z);
            // 임시 회전
            tmpDir = Vector3.RotateTowards(transform.forward, tmpDir, Time.deltaTime * rotateSpeed, 0);
            transform.rotation = Quaternion.LookRotation(tmpDir.normalized);
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            tmpDir = -(transform.right);
            float x = transform.position.x;
            x -= Time.deltaTime * moveSpeed;
            transform.position = new Vector3(x, transform.position.y, transform.position.z);
            // 임시 회전
            tmpDir = Vector3.RotateTowards(transform.forward, tmpDir, Time.deltaTime * rotateSpeed, 0);
            transform.rotation = Quaternion.LookRotation(tmpDir.normalized);
        }

        
    }
}
