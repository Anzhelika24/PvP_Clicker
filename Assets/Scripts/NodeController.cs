using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum NodeOwner
{
    Neutral = -1,
    Player0 = 0,
    Player1 = 1,
}

public class NodeController : MonoBehaviour
{
    [Header("Gameplay")]
    public int value;                          
    public NodeOwner owner = NodeOwner.Neutral;
    public List<NodeController> neighbors = new List<NodeController>(); 

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;
    public TextMeshProUGUI valueText;

    private GameManager gameManager;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        UpdateVisual();
    }

    public void SetRandomValue(int minValue, int maxValue)
    {
        value = Random.Range(minValue, maxValue + 1);
        UpdateVisual();
    }

    public void SetOwner(NodeOwner newOwner)
    {
        owner = newOwner;
        UpdateVisual();
    }


    public bool IsNeighborOfOwner(int playerId)
    {
        foreach (var n in neighbors)
        {
            if ((int)n.owner == playerId)
                return true;
        }
        return false;
    }

    public void OnClicked(int playerId)
    {
        gameManager.HandleNodeClick(this, playerId);
    }

    public void UpdateVisual()
    {
        if (spriteRenderer != null && gameManager != null)
        {
            if (owner == NodeOwner.Neutral)
                spriteRenderer.color = Color.gray;
            else
                spriteRenderer.color = gameManager.GetPlayerColor((int)owner);
        }

        if (valueText != null)
            valueText.text = value.ToString();
    }

    private void OnMouseDown()
    {
        if (gameManager != null)
        {
            gameManager.OnNodeClickedFromScene(this);
        }
    }
}
