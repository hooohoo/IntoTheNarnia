using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject _Camera;
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
        Move();
    }   // end Update()

    // 이동 함수
    void Move()
    {
        // 조이스틱 입력 경우

        // 키보드 입력 경우
        if (Input.GetKey(KeyCode.UpArrow))
        {
            float z = transform.position.z;
            z += Time.deltaTime * moveSpeed;
            transform.position = new Vector3(transform.position.x, transform.position.y, z);
            // 임시 회전
            tmpDir = transform.forward;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            float z = transform.position.z;
            z -= Time.deltaTime * moveSpeed;
            transform.position = new Vector3(transform.position.x, transform.position.y, z);
            // 임시 회전
            tmpDir = -(transform.forward);
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            float x = transform.position.x;
            x += Time.deltaTime * moveSpeed;
            transform.position = new Vector3(x, transform.position.y, transform.position.z);
            // 임시 회전
            tmpDir = transform.right;
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            float x = transform.position.x;
            x -= Time.deltaTime * moveSpeed;
            transform.position = new Vector3(x, transform.position.y, transform.position.z);
            // 임시 회전
            tmpDir = -(transform.right);
        }
        tmpDir = Vector3.RotateTowards(transform.forward, tmpDir, Time.deltaTime * rotateSpeed, 0);
        transform.rotation = Quaternion.LookRotation(tmpDir.normalized);
        
        //Debug.Log("rotation : " + transform.rotation);
    }
}
