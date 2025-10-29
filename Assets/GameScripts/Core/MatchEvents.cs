using System;
using System.Collections.Generic;

public class MatchEvents
{
    public Dictionary<MatchEventType, Delegate> events = new Dictionary<MatchEventType, Delegate>();

    public enum MatchEventType
    {
        OnTeamScored,
        OnRoundReset
    }

    // --- Adds an event ---
    public void AddEvent(MatchEventType type, Action action)
    {
        if (events.ContainsKey(type))
            events[type] = Delegate.Combine(events[type], action);
        else
            events[type] = action;
    }

    // --- Adds an event with parameter ---
    public void AddEvent<T>(MatchEventType type, Action<T> action)
    {
        if (events.ContainsKey(type))
            events[type] = Delegate.Combine(events[type], action);
        else
            events[type] = action;
    }

    // --- Triggers event ---
    public void TriggerEvent(MatchEventType type)
    {
        if (events.TryGetValue(type, out var del))
            (del as Action)?.Invoke();
    }

    // --- Triggers event with parameter ---
    public void TriggerEvent<T>(MatchEventType type, T param)
    {
        if (events.TryGetValue(type, out var del))
            (del as Action<T>)?.Invoke(param);
    }
}