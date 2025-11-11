using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class HandCardManager : MonoBehaviour
{
    public static HandCardManager instance;
    
    public int maxActionPointsInHand;
    private int currentActionPointsInHand;
    public Transform cardHolder;
    public GameObject cardPrefab;
    
    public ActionsListSO actionsList;
    
    private List<Card> currentHandCards = new List<Card>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        MatchManager.matchEvents.AddEvent<Team.TeamType>(MatchEvents.MatchEventType.OnTurnStart, OnTurnChange);
    }

    public void DrawCards()
    {
        StartCoroutine(DrawCardsRoutine());
    }

    IEnumerator DrawCardsRoutine()
    {
        DebugLogger.Log("Drawing Cards");
        
        currentHandCards.Clear();
        currentActionPointsInHand = 0;

        while (currentActionPointsInHand < maxActionPointsInHand)
        {
            int randomCard =  Random.Range(0, actionsList.actionList.Count);
            ActionSO action = actionsList.actionList[randomCard];
            
            if (currentActionPointsInHand + action.actionCost <= maxActionPointsInHand)
            {
                currentActionPointsInHand += action.actionCost;
                SpawnCard(action);
            }
            else if(currentActionPointsInHand != maxActionPointsInHand)
            {
                DrawCards();
                yield break;
            }
            else
            {
                break;
            }
            
            yield return null;
        }

        CheckCards();
    }

    public void SpawnCard(ActionSO actionData)
    {
        GameObject newCard = Instantiate(cardPrefab, cardHolder);
        Card card = newCard.GetComponent<Card>();
        card.SetUpCard(actionData);
        currentHandCards.Add(card);
    }

    public void ClearCards()
    {
        currentActionPointsInHand = 0;
        
        foreach (Transform card in cardHolder.transform)
        {
            Destroy(card.gameObject);
        }
    }

    public void ToggleCardsHolder(bool toggle)
    {
        cardHolder.gameObject.SetActive(toggle);
    }

    public void CheckCards()
    {
        TeamManager.events.TriggerEvent(TeamEvents.TeamEventType.CheckConditions, Team.TeamType.Player);

        foreach (var card in currentHandCards)
        {
            card.HandleStates();
        }
    }

    private void OnTurnChange(Team.TeamType teamType)
    {
        DebugLogger.Log($"Turn changed to {teamType}", "yellow");
        if (teamType == Team.TeamType.Player)
        {
            DrawCards();
        }
        
        ClearCards();
    }
}
