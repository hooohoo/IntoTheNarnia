using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderTest : MonoBehaviour
{
    public Collider _playerCollider;

    void Start()
    {
        _playerCollider = GameManager.Obj._player.GetComponent<Collider>();
    }

    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            Debug.Log("Ãæµ¹");
        }
    }
}
