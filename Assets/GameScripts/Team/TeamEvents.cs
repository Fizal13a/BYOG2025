using System;
using System.Collections.Generic;
using UnityEngine;

public class TeamEvents
{
    public Dictionary<TeamEventType, Action> events;

    public TeamEvents()
    {
        events = new Dictionary<TeamEventType, Action>();
    }

    public enum TeamEventType
    {
        CheckConditions,
        ResetPosition
    }
    
    // --- Adds an event ---
    public void AddEvent(TeamEventType type, Action action)
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
    public void TriggerEvent(TeamEventType type)
    {
        if (events.TryGetValue(type, out Action action))
        {
            action?.Invoke();
        }
    }
}
