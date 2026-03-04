using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class GlobalInputs : MonoBehaviour
{
    private InputActions input;
    private MenuUIManager _menuUI;

    private void Awake()
    {
        input = new InputActions();
        _menuUI = FindFirstObjectByType<MenuUIManager>();
    }

    private void OnEnable()
    {
        input.Global.Enable();
        input.Global.Return.performed += OnReturn;
    }

    private void OnDisable()
    {
        input.Global.Return.performed -= OnReturn;
        input.Global.Disable();
    }

    private void OnReturn(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        InputDevice device = ctx.control.device;
        var player = PlayerInput.all.FirstOrDefault(p => p.devices.Contains(device));

        if (_menuUI.isSelectingLevel)
        {
            if (player.currentActionMap.enabled)
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