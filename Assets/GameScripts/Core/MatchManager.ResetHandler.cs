using System;
using System.Collections;
using UnityEngine;

public partial class MatchManager : MonoBehaviour
{
    #region Reset

    public void ResetRound()
    {
        StartCoroutine(DelayForReset());
        StopAllTurns();
    }

    IEnumerator DelayForReset()
    {
        yield return new WaitForSeconds(3f);
        ResetMatch();
    }

    public void ResetMatch()
    {
        ResetActionPoints();

        if (scoredTeam == Team.TeamType.Player)
        {
            SetPlayerScore();
            StartCoroutine(DelayForTurn(() => StartTurn(playerTurnHandler)));
        }
        else
        {
            SetOpponentScore();
            StartCoroutine(DelayForTurn(() => StartTurn(opponentTurnHandler)));
        }
    }

    IEnumerator DelayForTurn(Action turn)
    {
        yield return new WaitForSeconds(3f);
        turn.Invoke();
    }
    #endregion
}
