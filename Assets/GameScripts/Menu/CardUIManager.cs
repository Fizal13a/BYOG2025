using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUIManager : MonoBehaviour
{
    [Header("Card Setup")]
    public GameObject cardPrefab;
    public List<CardData> cardDataList;

    // Use a single RectTransform for each area
    [Header("Containers")]
    public RectTransform allCardsPanel;
    public RectTransform selectedCardsPanel;

    [Header("Data Lists")]
    public List<CardData> allCardDataList;
    public List<CardData> selectedCardDataList;

    [Header("All Cards Layout Settings")]
    public float all_cardWidth = 200f;
    public float all_cardHeight = 300f;
    public float all_horizontalSpacing = 25f;
    public float all_verticalSpacing = 25f;
    public Vector2 all_panelPadding = new Vector2(20f, 20f);

    [Header("Selected Cards Layout Settings")]
    public float selected_cardWidth = 200f;
    public float selected_cardHeight = 300f;
    public float selected_horizontalSpacing = 25f;
    public float selected_verticalSpacing = 25f;
    public Vector2 selected_panelPadding = new Vector2(20f, 20f);

    [Header("Pooling")]
    public int initialPoolSize = 20;
    private List<GameObject> cardPool = new List<GameObject>();

    public static CardUIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitialSetup();
        CreateCardPool();
        UpdateAllDisplays();
    }

    private void InitialSetup()
    {
        allCardDataList = new List<CardData>(cardDataList);
        selectedCardDataList = new List<CardData>();
    }

    private void CreateCardPool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject cardInstance = Instantiate(cardPrefab, transform);
            cardInstance.SetActive(false);
            cardPool.Add(cardInstance);
        }
    }

    public void AddSelectedCards(CardData cardData)
    {
        if (selectedCardDataList.Count >= 3 || selectedCardDataList.Contains(cardData))
        {
            return;
        }

        selectedCardDataList.Add(cardData);
        allCardDataList.Remove(cardData);

        UpdateAllDisplays();
    }

    public void RemoveSelectedCards(CardData cardData)
    {
        if (selectedCardDataList.Count <= 0 || !selectedCardDataList.Contains(cardData))
        {
            return;
        }

        selectedCardDataList.Remove(cardData);
        allCardDataList.Add(cardData);

        UpdateAllDisplays();
    }

    public void UpdateAllDisplays()
    {
        foreach (var card in cardPool)
        {
            card.SetActive(false);
        }

        int poolIndex = 0;

        // Call the updated function for the main card list
        poolIndex = DisplayCardList(allCardsPanel, allCardDataList, poolIndex, all_cardWidth, all_cardHeight, all_horizontalSpacing, all_verticalSpacing, all_panelPadding);

        // Call the updated function for the selected card list
        DisplayCardList(selectedCardsPanel, selectedCardDataList, poolIndex, selected_cardWidth, selected_cardHeight, selected_horizontalSpacing, selected_verticalSpacing, selected_panelPadding);
    }

    // This function now takes a single "container" RectTransform
    private int DisplayCardList(RectTransform container, List<CardData> dataList, int startingPoolIndex, float cardWidth, float cardHeight, float hSpacing, float vSpacing, Vector2 padding)
    {
        if (container == null || dataList == null)
        {
            Debug.LogError("The container or data list is not assigned.");
            return startingPoolIndex;
        }

        float containerWidth = container.rect.width;
        float currentX = padding.x;
        float currentY = -padding.y;
        int poolIndex = startingPoolIndex;

        for (int i = 0; i < dataList.Count; i++)
        {
            if (poolIndex >= cardPool.Count)
            {
                GameObject newCard = Instantiate(cardPrefab, transform);
                cardPool.Add(newCard);
                Debug.LogWarning("Card pool was too small. Instantiated a new card.");
            }

            GameObject cardInstance = cardPool[poolIndex];
            cardInstance.transform.SetParent(container, false);

            if (currentX + cardWidth > containerWidth - padding.x)
            {
                currentX = padding.x;
                currentY -= (cardHeight + vSpacing);
            }

            RectTransform rect = cardInstance.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.sizeDelta = new Vector2(cardWidth, cardHeight);
            rect.anchoredPosition = new Vector2(currentX, currentY);

            CardData data = dataList[i];

            cardInstance.name = data.cardName;
            cardInstance.GetComponentInChildren<TMP_Text>().text = data.cardName;
            cardInstance.GetComponentInChildren<Image>().sprite = data.cardImage;
            cardInstance.GetComponent<MenuCardHandler>().cardData = data;

            cardInstance.SetActive(true);

            currentX += (cardWidth + hSpacing);
            poolIndex++;
        }

        return poolIndex;
    }
}