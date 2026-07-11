public abstract class SharedSingleton<T>
    where T : class
{
    public static T Instance { get; set; }
}
