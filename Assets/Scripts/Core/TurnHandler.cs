using UnityEngine;

[System.Serializable]
public class TurnHandler
{
    public string TeamName { get; private set; }
    public int CurrentAP { get; private set; }

    public TurnHandler(string teamName)
    {
        TeamName = teamName;
        CurrentAP = 0;
    }

    public void BeginTurn(int baseAP, int maxCarryOver)
    {
        CurrentAP = Mathf.Min(CurrentAP + baseAP, maxCarryOver);
    }

    public void UseActionPoints(int amount)
    {
        CurrentAP = Mathf.Max(0, CurrentAP - amount);
    }

    public bool CanAfford(int cost)
    {
        return CurrentAP >= cost;
    }

    public void SetCurrentActionPoints(int amount)
    {
        CurrentAP = amount;
    }
}