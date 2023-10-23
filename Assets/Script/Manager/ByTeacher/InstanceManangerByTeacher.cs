using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 인스턴스만 생성해주는 클래스
// 생성해서 갖고 있는다.
public class InstanceManangerByTeacher : SingleTon<InstanceManangerByTeacher>
{
    public List<Character<MONSTER>> mobList;   // 몬스터 저장
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
        //  캐릭터 생성위치는 파일에 저장되어 있거나 서버로부터 내려받는다.
        player.transform.position = GameHelper.GetHeightMapPos(Vector3.zero);
        player.transform.SetParent(parent);
    }

    /*
     // 내가 만든 함수
    public void CreateMonster(string name, Transform parent)
    {
        GameObject rcObj = ResourceManager.instance.GetRcCharacter(name);
        GameObject createObj = GameObject.Instantiate<GameObject>(rcObj);
        Character<MONSTER> mob = createObj.AddComponent<Monster>();
        mob.gameObject.layer = LayerMask.NameToLayer("Monster");
        mob.tag = "Monster";
        //  캐릭터 생성위치는 파일에 저장되어 있거나 서버로부터 내려받는다.
        mob.transform.position = GameHelper.GetHeightMapPos(Vector3.zero);
        mob.transform.SetParent(parent);
        mobList.Add(mob);
    }
    */

    public void CreateMonster(string fieldName, Transform parent)
    {
        // 테이블에서 로드한 데이터로 몬스터를 생성 또는 서버에서 내려받는 데이터를 기반으로 생성
        // 임시) 테이블이 아닌 10마리의 몬스터의 인스턴스를 생성
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
