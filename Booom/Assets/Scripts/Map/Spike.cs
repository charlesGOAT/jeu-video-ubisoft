using UnityEngine;

public class Spike : Tile
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            float playerDirectionX = player.transform.position.x - transform.position.x;
            float playerDirectionY = player.transform.position.y - transform.position.y;

            if (Mathf.Abs(playerDirectionX) > Mathf.Abs(playerDirectionY)) 
            {
                if (playerDirectionX >= 0)
                {
                    player.OnHit(Vector2Int.right);
                }
                else 
                {
                    player.OnHit(Vector2Int.left);
                }
            }
            else 
            {
                if (playerDirectionY >= 0)
                {
                    player.OnHit(Vector2Int.up);
                }
                else 
                {
                    player.OnHit(Vector2Int.down);
                }
            }

        }
    }
}
