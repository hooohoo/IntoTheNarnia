using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_QuestBoxController : MonoBehaviour
{
    void Start()
    {
        //
    }

    void Update()
    {
        //
    }

    // Test Code
    public void OnTriggerEnter(Collider collider)
    {
        // �÷��̾� �ݶ��̴��� �浹���� ��
        if(collider.CompareTag(Define.TagName.Player.ToString()))
        {
            Debug.Log("����");
            // #1
            // ������ ��� ����
            GameManager.Obj._theSiblingsRoomDoor._openOrLocked = true;
            // �������� �� Ż���ϵ���
            while(!GameManager.Quest._successOrNot)
            {
                Debug.Log("����Ʈ ������");
                // ������(TheSiblingsRoom) �� ������ ����Ʈ ����
                if(GameManager.Obj._theSiblingsRoomDoor._openOrClose)
                {
                    // �����޼��� UI�� �ٲ� ��
                    Debug.Log("ù ��° ����Ʈ ����!");
                    GameManager.Quest._successOrNot = true;
                }
            }
            // ���� ����Ʈ�� ���� false�� �缳��
            GameManager.Quest._successOrNot = false;
        }
    }
}
