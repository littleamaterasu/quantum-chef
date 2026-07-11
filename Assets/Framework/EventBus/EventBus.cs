using System;
using System.Collections.Generic;

public static class EventBus
{
    private static readonly Dictionary<Type, Delegate> eventMap = new();
    private static readonly Dictionary<Type, Delegate> requestMap = new();

    #region Event

    public static void Subscribe<T>(Action<T> callback)
    {
        var type = typeof(T);
        if (eventMap.TryGetValue(type, out var del))
            eventMap[type] = Delegate.Combine(del, callback);
        else
            eventMap[type] = callback;
    }

    public static void Unsubscribe<T>(Action<T> callback)
    {
        var type = typeof(T);

        if (!eventMap.TryGetValue(type, out var del))
            return;

        var newDel = Delegate.Remove(del, callback);

        if (newDel == null)
            eventMap.Remove(type);
        else
            eventMap[type] = newDel;
    }

    public static void Publish<T>(T evt)
    {
        if (eventMap.TryGetValue(typeof(T), out var del))
            (del as Action<T>)?.Invoke(evt);
    }

    #endregion

    #region Request

    public static void SubscribeRequest<TRequest, TResult>(Func<TRequest, TResult> callback)
    {
        requestMap[typeof(TRequest)] = callback;
    }

    public static void UnsubscribeRequest<TRequest>()
    {
        requestMap.Remove(typeof(TRequest));
    }

    public static TResult Request<TRequest, TResult>(TRequest request)
    {
        if (requestMap.TryGetValue(typeof(TRequest), out var del))
            return ((Func<TRequest, TResult>)del)(request);

        throw new Exception($"No handler for {typeof(TRequest).Name}");
    }

    #endregion
}