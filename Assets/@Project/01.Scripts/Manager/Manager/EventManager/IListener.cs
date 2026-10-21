using System;

public interface IListener<TEnum> where TEnum : Enum
{
    void OnEvent(TEnum eventType, object param = null);
}
