using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
        if (SceneManager.GetActiveScene().name == "EndGame") return;
        InputDevice device = ctx.control.device;
        var player = PlayerInput.all.FirstOrDefault(p => p.devices.Contains(device));

        if (player == null) return;
        
        float joinTime = LobbyManager.JoinTimes[player];
        if (Time.time - joinTime < 0.1f)
            return;
        
        if (player.inputIsActive && _menuUI.isNotMainMenu)
        {
            _menuUI.ReturnToMainMenu();
        }
        else
        {
            Destroy(player.gameObject);
        }
    }
}