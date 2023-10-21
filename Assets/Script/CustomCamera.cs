using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCamera : MonoBehaviour
{
    public GameObject player;
    public Camera customCam;
    Vector3 playerPos;
    Vector3 camPos;
    Vector3 offset;

    void Awake()
    {
        customCam = Camera.main;
        playerPos = player.transform.position;
        camPos = customCam.transform.position;
    }

    void Start()
    {
        offset = camPos - playerPos;
    }

    void Update()
    {
        playerPos = player.transform.position;
    }

    void LateUpdate()
    {
        customCam.transform.position = playerPos + offset;
    }
}
