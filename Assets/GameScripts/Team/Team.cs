using System;

[Serializable]
public class Team
{
    public enum TeamType
    {
        Player, Opponent
    }
    
    public TeamType teamType;
    public string teamName;
}