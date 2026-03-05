using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class GlobalInputs : MonoBehaviour
{
    private InputActions _input;
    private MenuUIManager _menuUI;

    private void Awake()
    {
        _input = new InputActions();
        _menuUI = FindFirstObjectByType<MenuUIManager>();
    }

    private void OnEnable()
    {
        _input.Global.Enable();
        _input.Global.Return.performed += OnReturn;
    }

    private void OnDisable()
    {
        _input.Global.Return.performed -= OnReturn;
        _input.Global.Disable();
    }

    private void OnReturn(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        InputDevice device = ctx.control.device;
        var player = PlayerInput.all.FirstOrDefault(p => p.devices.Contains(device));

        if (_menuUI.isSelectingLevel)
        {
            if (player.inputIsActive)
            {
                _menuUI.ReturnToMainMenu();
            }
        }
        else
        {
            Destroy(player.gameObject);
        }
    }
}