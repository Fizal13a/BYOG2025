using UnityEngine;

public class MenuCardHandler : MonoBehaviour
{
    public CardData cardData;

    public void OnCardLeftClicked()
    {
        if (cardData != null)
        {
            CardUIManager.Instance.AddSelectedCards(cardData);
        }
    }

    public void OnCardRightClicked()
    {
        if (cardData != null)
        {
            CardUIManager.Instance.RemoveSelectedCards(cardData);
        }
    }
}
