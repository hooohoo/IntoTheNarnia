using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// �ν��Ͻ��� �������ִ� Ŭ����
// �����ؼ� ���� �ִ´�.
public class InstanceManangerByTeacher : SingleTon<InstanceManangerByTeacher>
{
    public List<Character<MONSTER>> mobList;   // ���� ����
    public Player2 player;
    public void Initialize()
    {
        mobList = new List<Character<MONSTER>>();
        ResourceManagerByTeacher.instance.LoadCharacter();
    }

    public void CreatePlayer(string name, Transform parent)
    {
        GameObject rcObj = ResourceManagerByTeacher.instance.GetRcCharacter(name);
        GameObject createObj = GameObject.Instantiate<GameObject>(rcObj);
        player = createObj.AddComponent<Player2>();
        player.gameObject.layer = LayerMask.NameToLayer("Player");
        player.tag = "Player";
        //  ĳ���� ������ġ�� ���Ͽ� ����Ǿ� �ְų� �����κ��� �����޴´�.
        player.transform.position = GameHelper.GetHeightMapPos(Vector3.zero);
        player.transform.SetParent(parent);
    }

    /*
     // ���� ���� �Լ�
    public void CreateMonster(string name, Transform parent)
    {
        GameObject rcObj = ResourceManager.instance.GetRcCharacter(name);
        GameObject createObj = GameObject.Instantiate<GameObject>(rcObj);
        Character<MONSTER> mob = createObj.AddComponent<Monster>();
        mob.gameObject.layer = LayerMask.NameToLayer("Monster");
        mob.tag = "Monster";
        //  ĳ���� ������ġ�� ���Ͽ� ����Ǿ� �ְų� �����κ��� �����޴´�.
        mob.transform.position = GameHelper.GetHeightMapPos(Vector3.zero);
        mob.transform.SetParent(parent);
        mobList.Add(mob);
    }
    */

    public void CreateMonster(string fieldName, Transform parent)
    {
        // ���̺��� �ε��� �����ͷ� ���͸� ���� �Ǵ� �������� �����޴� �����͸� ������� ����
        // �ӽ�) ���̺��� �ƴ� 10������ ������ �ν��Ͻ��� ����
        for(int i = 0; i < 10; i++)
        {
            GameObject rcObj = ResourceManagerByTeacher.instance.GetRcCharacter("Monster");
            GameObject createObj = GameObject.Instantiate<GameObject>(rcObj);
            Character<MONSTER> mob = createObj.AddComponent<Monster>();
            MONSTER tmp = new MONSTER();
            tmp.name = "Monster_" + i;
            tmp.area = Area.COUNTRY1;
            //tmp.type = 1;

            mob.data = tmp;
            Vector3 spawnPos = new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
            createObj.transform.position = GameHelper.GetHeightMapPos(spawnPos);
            createObj.transform.SetParent(parent);
            
            mobList.Add(mob);

        }
    }
}
