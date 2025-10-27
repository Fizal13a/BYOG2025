using System;
using System.Collections.Generic;
using UnityEngine;

public class MatchEvents
{
    public Dictionary<MatchEventType, Action> events;

    public MatchEvents()
    {
        events = new Dictionary<MatchEventType, Action>();
    }

    public enum MatchEventType
    {
        OnStartMatch,
        OnTurnEnd,
        OnTurnStart,
        OnGoalScored
    }
    
    // --- Adds an event ---
    public void AddEvent(MatchEventType type, Action action)
    {
        if (events.ContainsKey(type))
        {
            events[type] += action;
        }
        else
        {
            events[type] = action;
        }
    }
    
    // --- Triggers an event ---
    public void TriggerEvent(MatchEventType type)
    {
        if (events.TryGetValue(type, out Action action))
        {
            action?.Invoke();
        }
    }
}