using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ���� ������ �̵��� �� �̿�Ǵ� ��Ʈ�ѷ�
// BoxCollider, Rigidbody ����� ������Ʈ�� �����Ǿ� ���
public class NextMapController : MonoBehaviour
{
    // ����� �� �̸�
    public Define.SceneName _linkedScene;

    void Start()
    {
        //
    }

    void Update()
    {
        //
    }

    // ���� � ���� �־����� üũ�ϴ� �Լ�
    private void RecentSceneCheck()
    {
        //
    }

    // �÷��̾� �ݶ��̴� �ε����� ���� ������ �Ѿ��
    private void OnTriggerEnter(Collider other)
    {
        // �±װ� Player �̸�
        if(other.CompareTag(Define.TagName.Player.ToString()))
        {
            // �� �ε�
            GameManager.Scene.LoadScene(_linkedScene);
        }
    }
}
