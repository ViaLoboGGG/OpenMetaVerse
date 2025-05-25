using System;

public static class EventBus<T>
{
    public static event Action<T> OnEvent;

    public static void Raise(T value)
    {
        OnEvent?.Invoke(value);
    }

    public static void Subscribe(Action<T> handler) => OnEvent += handler;
    public static void Unsubscribe(Action<T> handler) => OnEvent -= handler;
}