using System;
using System.Collections;
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
                continue;
            }
            else
            {
                break;
            }
            
            yield return null;
        }
    }

    public void SpawnCard(ActionSO actionData)
    {
        GameObject newCard = Instantiate(cardPrefab, cardHolder);
        Card card = newCard.GetComponent<Card>();
        card.SetUpCard(actionData);
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
}
