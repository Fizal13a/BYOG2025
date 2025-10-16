using UnityEngine;
using UnityEngine.EventSystems;

public partial class PlayerController : MonoBehaviour
{
    private bool canSelect = false;
    
    private void HandleActionSelection()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (canSelect && Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, tileLayer))
            {
                GridTile clickedTile = hit.collider.GetComponent<GridTile>();
                if (clickedTile != null && GridGenerator.instance.highlightedTiles.Contains(clickedTile))
                {
                    DebugLogger.Log("Target Tile Selected", "yellow");

                    switch(currentAction.action)
                    {
                        case ActionData.Actions.Move:
                            MoveToTile(clickedTile);
                            break;

                        case ActionData.Actions.Pass:
                            PassToPlayer(clickedTile);
                            break;
                    }

                    
                    GridGenerator.instance.ClearHighlightedTiles();
                    canSelect = false;
                }
            }
        }

    }
}
