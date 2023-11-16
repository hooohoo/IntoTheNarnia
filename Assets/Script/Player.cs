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

    // �̵� �Լ�
    void Move()
    {
        // ���̽�ƽ �Է� ���

        // Ű���� �Է� ���
        if (Input.GetKey(KeyCode.UpArrow))
        {
            float z = transform.position.z;
            z += Time.deltaTime * moveSpeed;
            transform.position = new Vector3(transform.position.x, transform.position.y, z);
            // �ӽ� ȸ��
            tmpDir = transform.forward;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            float z = transform.position.z;
            z -= Time.deltaTime * moveSpeed;
            transform.position = new Vector3(transform.position.x, transform.position.y, z);
            // �ӽ� ȸ��
            tmpDir = -(transform.forward);
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            float x = transform.position.x;
            x += Time.deltaTime * moveSpeed;
            transform.position = new Vector3(x, transform.position.y, transform.position.z);
            // �ӽ� ȸ��
            tmpDir = transform.right;
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            float x = transform.position.x;
            x -= Time.deltaTime * moveSpeed;
            transform.position = new Vector3(x, transform.position.y, transform.position.z);
            // �ӽ� ȸ��
            tmpDir = -(transform.right);
        }
        tmpDir = Vector3.RotateTowards(transform.forward, tmpDir, Time.deltaTime * rotateSpeed, 0);
        transform.rotation = Quaternion.LookRotation(tmpDir.normalized);
        
        //Debug.Log("rotation : " + transform.rotation);
    }
}
