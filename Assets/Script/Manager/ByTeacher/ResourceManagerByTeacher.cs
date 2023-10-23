using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManagerByTeacher : SingleTon<ResourceManagerByTeacher>
{
    List<GameObject> rcCharList;
    const string charFolder = "Character";
    public void LoadCharacter()
    {
        rcCharList = new List<GameObject>();
        GameObject [] objs = Resources.LoadAll<GameObject>(charFolder);

        foreach(GameObject one in objs)
        {
            rcCharList.Add(one);
        }
    }

    public GameObject GetRcCharacter(string name)
    {
        return rcCharList.Find(o=>(o.name.Equals(name)));
    }
}
