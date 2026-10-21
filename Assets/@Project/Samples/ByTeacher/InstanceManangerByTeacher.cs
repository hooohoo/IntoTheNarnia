using System.Collections.Generic;
using UnityEngine;

public sealed class InstanceManangerByTeacher : SingleTon<InstanceManangerByTeacher>
{
    private readonly List<Character<SampleMonsterData>> monsters = new List<Character<SampleMonsterData>>();
    private readonly ResourceManagerByTeacher resources = new ResourceManagerByTeacher();
    public Player2 Player { get; private set; }
    public void Initialize() { monsters.Clear(); resources.LoadCharacter(); }
    public void CreatePlayer(string name, Transform parent)
    {
        var instance = Object.Instantiate(resources.GetRcCharacter(name), parent);
        Player = instance.GetComponent<Player2>();
        if (Player == null) Player = instance.AddComponent<Player2>();
        instance.layer = LayerMask.NameToLayer("Player");
        instance.tag = "Player";
        instance.transform.position = GameHelper.GetHeightMapPos(Vector3.zero);
    }
    public void CreateMonster(string fieldName, Transform parent)
    {
        for (int i = 0; i < 10; i++)
        {
            var instance = Object.Instantiate(resources.GetRcCharacter("Monster"), parent);
            var monster = instance.GetComponent<Monster>();
            if (monster == null) monster = instance.AddComponent<Monster>();
            monster.Data = new SampleMonsterData { Name = "Monster_" + i, Area = Area.COUNTRY1 };
            instance.transform.position = GameHelper.GetHeightMapPos(new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f)));
            monsters.Add(monster);
        }
    }
}
