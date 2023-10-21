using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWithStick : MonoBehaviour
{
    float moveSpeed;

    // Start is called before the first frame update
    void Start()
    {
        moveSpeed = 3f;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 dir = Joystick.instance.dir.normalized;
        Vector3 tmp = transform.position;
        tmp.x += dir.x * Time.deltaTime * moveSpeed;
        tmp.y += dir.y * Time.deltaTime * moveSpeed;
        tmp.z += dir.z * Time.deltaTime * moveSpeed;
        transform.position = tmp;
    }
}
