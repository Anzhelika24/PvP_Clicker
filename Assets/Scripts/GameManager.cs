using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{   [Header("End Game UI")]
    public GameObject winPanel;
    public TextMeshProUGUI winText;

    [Header("Players")]
    public List<PlayerData> players = new List<PlayerData>();
    public int currentPlayerIndex = 0;

    [Header("Nodes")]
    public List<NodeController> allNodes = new List<NodeController>();
    public int minStartValue = 1;
    public int maxStartValue = 10;

    [Header("UI")]
    public TextMeshProUGUI statusText;

    private bool gameOver = false;

    private void Start()
    {
        if (winPanel != null)
            winPanel.SetActive(false);
        
        InitNodesRandom();
        AssignRandomOwners();
        UpdateStatusText();
        
    }

    public Color GetPlayerColor(int playerId)
    {
        var p = players.Find(pl => pl.playerId == playerId);
        return p != null ? p.color : Color.white;
    }

    private void InitNodesRandom()
    {
        foreach (var node in allNodes)
        {
            node.SetRandomValue(minStartValue, maxStartValue);
            node.SetOwner(NodeOwner.Neutral);
        }
    }

    private void AssignRandomOwners()
    {
        List<NodeController> freeNodes = new List<NodeController>(allNodes);

        foreach (var player in players)
        {
            if (freeNodes.Count == 0) break;
            int idx = Random.Range(0, freeNodes.Count);
            var node = freeNodes[idx];
            node.SetOwner((NodeOwner)player.playerId);
            freeNodes.RemoveAt(idx);
        }

        foreach (var node in freeNodes)
        {
            int roll = Random.Range(-1, players.Count);
            if (roll == -1)
                node.SetOwner(NodeOwner.Neutral);
            else
                node.SetOwner((NodeOwner)roll);
        }
    }

    public void OnNodeClickedFromScene(NodeController node)
    {
        if (gameOver) return;

        var currentPlayer = players[currentPlayerIndex];
        if (!currentPlayer.isAlive) return;

        HandleNodeClick(node, currentPlayer.playerId);
    }

    public void HandleNodeClick(NodeController node, int playerId)
    {
        if (gameOver) return;

        PlayerData player = players.Find(p => p.playerId == playerId);
        if (player == null || !player.isAlive) return;

        if ((int)node.owner == playerId)
        {
            node.value += player.clickPower;
            node.UpdateVisual();
            EndTurnCheck();
            return;
        }

        if (!node.IsNeighborOfOwner(playerId))
        {
            return;
        }

        if (node.value > 0)
        {
            node.value -= player.clickPower;
            if (node.value <= 0)
            {
                node.value = 0;
                node.SetOwner(NodeOwner.Neutral);
            }
            else
            {
                node.UpdateVisual();
            }
        }
        else
        {
            node.SetOwner((NodeOwner)playerId);
            node.value += player.clickPower;
        }

        node.UpdateVisual();
        CheckPlayersAlive();
        EndTurnCheck();
    }

    private void CheckPlayersAlive()
    {
        foreach (var player in players)
        {
            if (!player.isAlive) continue;

            bool hasNode = false;
            foreach (var node in allNodes)
            {
                if ((int)node.owner == player.playerId)
                {
                    hasNode = true;
                    break;
                }
            }

            if (!hasNode)
            {
                player.isAlive = false;
            }
        }

        int aliveCount = 0;
        PlayerData lastAlive = null;
        foreach (var p in players)
        {
            if (p.isAlive)
            {
                aliveCount++;
                lastAlive = p;
            }
        }

        if (aliveCount <= 1)
        {
            gameOver = true;

            string message;
            if (lastAlive != null)
                message = $"Победил: {lastAlive.playerName}";
            else
                message = "Ничья";

            if (statusText != null)
                statusText.text = message;

            if (winPanel != null)
                winPanel.SetActive(true);

            if (winText != null)
                winText.text = message;
        }
    }

    private void EndTurnCheck()
    {
        NextPlayer();
        UpdateStatusText();
    }

    private void NextPlayer()
    {
        int startIndex = currentPlayerIndex;
        do
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
            if (players[currentPlayerIndex].isAlive)
                break;
        } while (currentPlayerIndex != startIndex);
    }

    private void UpdateStatusText()
    {
        if (gameOver) return;

        var p = players[currentPlayerIndex];
        if (statusText != null) 
        {
            statusText.text = $"Ход: {p.playerName}";
            statusText.color = p.color;
        }
    }
    public void RestartGame()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
}
