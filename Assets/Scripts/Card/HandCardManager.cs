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
    
    private List<CardObj> currentHandCards = new List<CardObj>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        //DrawCards();
    }

    public void DrawCards()
    {
        StartCoroutine(DrawCardsRoutine());
    }

    IEnumerator DrawCardsRoutine()
    {
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
        CardObj card = newCard.GetComponent<CardObj>();
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
        PlayerController.instance.SetConditions();

        foreach (var card in currentHandCards)
        {
            card.HandleCardState();
        }
    }
}
