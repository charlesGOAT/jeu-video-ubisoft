using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float speed = 5f;
    
    private Vector2 _moveInput;

    private void Start()
    {
        
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        _moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnBomb(InputAction.CallbackContext ctx)
    {
        /*
         *  TODO: Bomb creation logic (We can keep the explosion logic in Bomb.cs)
         */
        if (ctx.performed)
        {
            Debug.Log("Bomb");
        }
    }
    
    private void Update()
    {
        Vector2 curMoveInput = _moveInput.normalized;

        Vector2 move = curMoveInput * (speed * Time.deltaTime);
        transform.position += new Vector3(move.y, 0, -move.x);
    }
}