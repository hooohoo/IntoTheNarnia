using UnityEngine;

public abstract class UI_Base : MonoBehaviour
{
    public string UIKey { get; private set; }
    public int SortOrder { get; private set; }
    public abstract void Init();
    public virtual void Open() { gameObject.SetActive(true); }
    public virtual void Close() { gameObject.SetActive(false); }
    public abstract void Release();
    public void SetUIKey(string key) { UIKey = key; }
    public void SetSortOrder(int order)
    {
        SortOrder = order;
        var canvas = GetComponent<Canvas>();
        if (canvas != null) canvas.sortingOrder = order;
    }
}
