using UnityEngine;

public class Trampoline : Tile
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
                    player.OnJump();
                }
                else
                {
                    player.OnJump();
                }
            }
            else
            {
                if (playerDirectionY >= 0)
                {
                    player.OnJump();
                }
                else
                {
                    player.OnJump();
                }
            }

        }
    }
}