using System;
using System.Collections.Generic;
using UnityEngine;

public class TeamEvents
{
    public Dictionary<TeamEventType, Delegate> events = new Dictionary<TeamEventType, Delegate>();

    public enum TeamEventType
    {
        CheckConditions,
        ResetPosition
    }
    
    // --- Adds an event ---
    public void AddEvent(TeamEventType type, Action action)
    {
        if (events.ContainsKey(type))
            events[type] = Delegate.Combine(events[type], action);
        else
            events[type] = action;
    }

    // --- Adds an event with parameter ---
    public void AddEvent<T>(TeamEventType type, Action<T> action)
    {
        if (events.ContainsKey(type))
            events[type] = Delegate.Combine(events[type], action);
        else
            events[type] = action;
    }

    // --- Triggers event ---
    public void TriggerEvent(TeamEventType type)
    {
        if (events.TryGetValue(type, out var del))
            (del as Action)?.Invoke();
    }

    // --- Triggers event with parameter ---
    public void TriggerEvent<T>(TeamEventType type, T param)
    {
        if (events.TryGetValue(type, out var del))
            (del as Action<T>)?.Invoke(param);
    }
}
