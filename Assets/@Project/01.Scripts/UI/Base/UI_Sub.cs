public abstract class UI_Sub : UI_Base
{
    public UI_Base OwnerUI { get; private set; }
    public void SetOwnerUI(UI_Base owner) { OwnerUI = owner; }
    public virtual void OnSpawn() { }
    public virtual void OnDespawn() { }
}
