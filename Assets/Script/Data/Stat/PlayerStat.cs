using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// �÷��̾� ����

// ===�߰�����===
// �̸�
// ����
// ������ ����ġ
// ���� ����ġ

[System.Serializable]
public class PlayerStat : Stat
{
    // �÷��̾� ���� �̱������� ����
    private static PlayerStat _instance;

    // null ��� getter
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

    // �̸�
    [SerializeField]
    private string _Name;

    // ����
    [SerializeField]
    private string _Job;

    // ���� ����ġ
    [SerializeField]
    private int _Exp;

    // ������ ����ġ
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
