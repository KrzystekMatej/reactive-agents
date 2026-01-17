using System.Collections.Generic;
using UnityEngine;
using System;

public class AgentContext : MonoBehaviour
{
    private Dictionary<Type, object> cache = new();

    void Awake()
    {
        foreach (var component in GetComponentsInChildren<AgentComponent>())
        {
            cache[component.GetType()] = component;
            component.Initialize(this);
        }
    }

    public T Get<T>(bool save = true)
    {
        if (!cache.TryGetValue(typeof(T), out var component))
        {
            component = GetComponentInChildren<T>();
            if (component != null && save) cache[typeof(T)] = component;
        }
        return (T)component;
    }
}
