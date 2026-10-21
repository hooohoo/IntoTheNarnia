// Historical teaching example, isolated from runtime service registration.
public class SingleTon<T> where T : class, new()
{
    private static T instance;
    public static T Instance => instance ?? (instance = new T());
}
