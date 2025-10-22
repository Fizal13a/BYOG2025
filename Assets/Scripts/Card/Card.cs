using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
   [Header("UI")]
   public Image actionImage;
   public TextMeshProUGUI actionName;
   public TextMeshProUGUI actionCost;

   public void SetUpCard(ActionSO actionData)
   {
      actionName.text = actionData.actionName;
      actionCost.text = actionData.actionCost.ToString();
   }
}
