using UnityEngine;

public class TutoManager : MonoBehaviour
{
    void Start()
    {
        foreach (var player in LobbyManager.JoinedPlayers.Values)
        {
            var moveAction = player.actions["Move"];
            moveAction.Disable();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
