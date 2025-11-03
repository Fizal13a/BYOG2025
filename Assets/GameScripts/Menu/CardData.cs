using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Card System/Card Data")]
public class CardData : ScriptableObject
{
    public string cardName;
    public Sprite cardImage;
    public string description;
}
