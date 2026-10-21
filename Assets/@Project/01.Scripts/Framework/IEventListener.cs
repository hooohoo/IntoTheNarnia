public interface IEventListener<T>
{
    void OnEvent(T message);
}
