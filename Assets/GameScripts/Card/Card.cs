using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public interface IDropTarget
{
    void OnCardDropped(ActionSO actionData);
}

public class Card : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI")]
    public Image actionImage;
    public TextMeshProUGUI actionName;
    public TextMeshProUGUI actionCost;
    
    private ActionSO currentAction;
    private CanvasGroup canvasGroup;
    private Canvas parentCanvas;
    private RectTransform rectTransform;

    private Transform originalParent;
    private Vector3 originalPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        parentCanvas = GetComponentInParent<Canvas>();
    }

    public void SetUpCard(ActionSO actionData)
    {
        currentAction = actionData;
        actionName.text = actionData.actionName;
        actionCost.text = actionData.actionCost.ToString();
    }

    public void HandleStates()
    {
        
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Save position and parent so we can restore later
        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;

        // Move card to top canvas layer
        transform.SetParent(parentCanvas.transform, true);

        // Make it semi-transparent and allow raycast pass-through
        canvasGroup.alpha = 0.7f;
        canvasGroup.blocksRaycasts = false;

        // Hide all other cards
        HandCardManager.instance.ToggleCardsHolder(false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (parentCanvas != null)
            rectTransform.anchoredPosition += eventData.delta / parentCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Restore visuals
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Restore parent and position
        transform.SetParent(originalParent, true);
        rectTransform.anchoredPosition = originalPosition;

        // Reactivate other cards
        HandCardManager.instance.ToggleCardsHolder(true);

        // 🔹 Cast a ray from the cursor into the 3D world
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Check if the hit object has a drop handler
            var dropTarget = hit.collider.GetComponent<Player>();
            if (dropTarget != null)
            {
                dropTarget.HandleStates(currentAction.actionType);
                Destroy(gameObject);
            }
        }
    }
}