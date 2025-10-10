using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private PlayerController controller;
    
    [Header("Grid Data")] 
    private Vector2Int GridPosition;
    
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private LayerMask tileLayer;
    
    private bool canSelect = false;

    public void SetUpPlayer(PlayerController playerController)
    {
        controller = playerController;
    }

    private void Update()
    {
        if (canSelect && Input.GetMouseButtonDown(0))
        {
            HandleTileClick();
        }
    }
    
    private void HandleTileClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, tileLayer))
        {
            GridTile clickedTile = hit.collider.GetComponent<GridTile>();
            if (clickedTile != null && GridGenerator.instance.highlightedTiles.Contains(clickedTile))
            {
                StartCoroutine(MoveToTile(clickedTile));
                GridGenerator.instance.ClearHighlightedTiles();
                canSelect = false;
            }
        }
    }
    
    private IEnumerator MoveToTile(GridTile targetTile)
    {
        Vector3 start = transform.position;
        Vector3 end = targetTile.WorldPosition; // assuming Tile has WorldPosition property
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        SetGridPosition(targetTile.GridPosition);
        //HandleStates(PlayerController.PlayerStates.Idle); // or next state
    }

    public void SetGridPosition(Vector2Int gridPosition)
    {
        GridPosition = gridPosition;
    }
    
    public Vector2Int GetGridPosition() => GridPosition;
    public void SetSelection(bool selection) => canSelect = selection;
}
