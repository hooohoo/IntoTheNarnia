using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniMapController : MonoBehaviour
{
    // 임시코드
    public Vector3 _worldSize = new Vector3(1900f, 0f, 1069f);
    public ScrollRect _scrollRect;
    public GameObject _playerIcon;
    public GameObject _plane;
    public GameObject _playerObject;

    void Start()
    {
        _worldSize = _plane.transform.GetComponent<MeshCollider>().bounds.size;
        //Debug.Log("plane size : " + _plane.transform.GetComponent<MeshCollider>().bounds.size);
        _scrollRect.normalizedPosition = new Vector2(0.5f, 0.5f);
        //Debug.Log("viewport.anchoredPosition : " + _scrollRect.viewport.anchoredPosition);
        //Debug.Log("content.sizeDelta : " + _scrollRect.content.sizeDelta);
    }

    void Update()
    {
        //Debug.Log("icon position : " + _playerIcon.transform.localPosition);
       //Debug.Log("normalizedPosition : " + _scrollRect.normalizedPosition);
        TryCalculate();
    }

    void TryCalculate()
    {
        Vector3 relativePos = _playerObject.transform.position - _plane.transform.position;
        //relativePos.x += 950f;
        relativePos.x += _worldSize.x * 0.5f;
        //relativePos.z += 1069f * 0.5f;
        relativePos.z += _worldSize.z * 0.5f;
        //Debug.Log("player 상대 위치 : " + relativePos);
        float playerX = Mathf.InverseLerp(0, _worldSize.x, relativePos.x);
        float playerY = Mathf.InverseLerp(0, _worldSize.z, relativePos.z);
        //Debug.Log("x : " + playerX + " y : " + playerY);
        _scrollRect.normalizedPosition = new Vector2(playerX, playerY);
    }
}
