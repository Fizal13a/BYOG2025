using UnityEngine;

[System.Serializable]
public class ActionData
{
    public enum Actions
    {
        Move, Pass, Tackle, Shoot, Dash
    }
    
    public Actions action;
    public int actionCost;
}
