using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// <T> -> 직업 여러개일 경우 대비
public class Character<T> : MonoBehaviour
{
    public T data { get; set; }


    void Start()
    {
        
    }

    void Update()
    {
        
    }
}

public struct CHARACTER
{
    public string name;
    public byte classType;
}
public enum Area
{
    NONE = 0,
    COUNTRY1,
    COUNTRY2,
    COUNTRY3
}

public struct MONSTER
{
    public string name;
    public Area area;
    public byte classType;
}

