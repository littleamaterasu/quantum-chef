public abstract class RuntimeSingleton<T>
    where T : new()
{
    private static T instance;

    public static T Instance
    {
        get
        {
            instance ??= new T();
            return instance;
        }
    }
}
