using UnityEngine;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    private void Start()
    {
        foreach (var player in LobbyManager.JoinedPlayers.Values)
        {
            SpawnMenuPlayer(player);
        }
    }

    public void SpawnMenuPlayer(in PlayerInput playerInput)
    {
        playerPrefab.layer = GameManager.Instance.CollisionLayers[playerInput.playerIndex];
        playerInput.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
    }

    public void PlayerMenuLeft(in int leavingIndex)
    {
        Vector2Int spawnPoint = GameManager.Instance.GridManager.playerSpawnPoints[leavingIndex];
        Tile tile = GameManager.Instance.GridManager.GetTileAtCoordinates(spawnPoint);
        tile.IsSpawn = false;
        tile.ChangeTileColor(PlayerEnum.None);
    }
}