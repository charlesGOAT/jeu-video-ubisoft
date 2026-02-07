using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float speed = 5f;
    private InputActions _inputActions;

    private void Awake()
    {
        _inputActions = new InputActions();
    }

    private void OnEnable()
    {
        _inputActions.Enable();
        _inputActions.Player.PlaceBomb.performed += OnBomb;
        _inputActions.Player.PickItem.performed += OnPickUp;
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        Vector2 curMoveInput = _inputActions.Player.Move.ReadValue<Vector2>();
        curMoveInput.Normalize();

        var move = curMoveInput * (speed * Time.deltaTime);
        transform.position += new Vector3(move.y, 0, -move.x);
    }

    private void OnBomb(InputAction.CallbackContext ctx)
    {
        /*
         *  TODO: Bomb creation logic (We can keep the explosion logic in Bomb.cs)
         */
        Debug.Log("Bomb");
    }

    private void OnPickUp(InputAction.CallbackContext ctx)
    {
        /*
         *  TODO: Pick up logic (calling Item.applyBuff() or something)
         */
        Debug.Log("Pick Up");
    }
    
    private void OnDisable()
    {
        _inputActions.Player.PlaceBomb.performed -= OnBomb;
        _inputActions.Player.PickItem.performed -= OnPickUp;
        _inputActions.Disable();
    }
}