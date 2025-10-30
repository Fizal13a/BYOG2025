using System;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;
using Unity.Cinemachine;

public class MatchData
{
    public enum MatchMode
    {
        Local,
        PlayOnline,
        PlayWithFriend
    }
    public MatchMode currentMode;
    public int MatchTime;
}

public partial class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    private MatchData matchData;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void Start()
    {
        matchData = new MatchData();
    }

    public void StartMatch(int mode)
    {
        switch (mode)
        {
            case 0:
                matchData.currentMode = MatchData.MatchMode.Local;
                break;
            case 1:
                matchData.currentMode = MatchData.MatchMode.PlayOnline;
                break;
            case 2:
                matchData.currentMode = MatchData.MatchMode.PlayWithFriend;
                break;
        }
    }

    public MatchData.MatchMode GetCurrentMode()
    {
        return matchData.currentMode;
    }
}
