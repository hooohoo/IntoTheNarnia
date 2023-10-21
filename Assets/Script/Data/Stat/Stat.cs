using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ������Ʈ(�÷��̾�, ����) ���� ����

// ���� ü��
// ���ݷ�
// ����
// �ִ� ü��
// ����

[System.Serializable]
public class Stat
{
    // ���� ü��
    [SerializeField]
    private int _Hp;
    
    // ���ݷ�
    [SerializeField]
    private int _Atk;
    
    // ����
    [SerializeField]
    private int _Def;
    
    // ���� ����
    [SerializeField]
    private int _Lv;

    // �ִ� ü��
    [SerializeField]
    private int _Max_Hp;


    public int Hp
    {
        get { return _Hp; }
        set { _Hp = value; }
    }

    public int Atk
    {
        get { return _Atk; }
        set { _Atk = value; }
    }
    
    public int Def
    {
        get { return _Def; }
        set { _Def = value; }
    }
    
    public int Lv
    {
        get { return _Lv; }
        set { _Lv = value; }
    }
    
    public int Max_Hp
    {
        get { return _Max_Hp; }
    }

}
