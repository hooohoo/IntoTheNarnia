using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 플레이어 스탯

// ===추가사항===
// 이름
// 직업
// 레벨업 경험치
// 현재 경험치

[System.Serializable]
public class PlayerStat : Stat
{
    // 플레이어 스탯 싱글톤으로 관리
    private static PlayerStat _instance;

    // null 대비 getter
    public static PlayerStat Instance()
    {
        if(_instance == null)
        {
            _instance = new PlayerStat();
        }
        return _instance;
    }

    // setter
    public static PlayerStat SetInstance
    {
        set { _instance = value; }
    }

    // 이름
    [SerializeField]
    private string _Name;

    // 직업
    [SerializeField]
    private string _Job;

    // 현재 경험치
    [SerializeField]
    private int _Exp;

    // 레벨업 경험치
    [SerializeField]
    private int _Lv_Exp;

    public string Name
    {
        get { return _Name; }
        set { _Name = value; }
    }

    public string Job
    {
        get { return _Job; }
        set { _Job = value; }
    }

    public int Exp
    {
        get { return _Exp; }
        set { _Exp = value; }
    }

    public int Lv_Exp
    {
        get { return _Lv_Exp; }
    }
}
