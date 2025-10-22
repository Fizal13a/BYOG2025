using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "ActionSO", menuName = "Scriptable Objects/ActionSO")]
public class ActionSO : ScriptableObject
{
    public Sprite cardBGSprite;
    public Sprite cardActionSprite;
    public int actionCost;
    public string actionName;
}
