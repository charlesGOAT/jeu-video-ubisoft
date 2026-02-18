using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTests
{
    private GameObject _playerGo;
    private Player _player;

    [SetUp]
    public void Setup()
    {
        _playerGo = new GameObject();
        _playerGo.AddComponent<CharacterController>();
        
        var input = _playerGo.AddComponent<PlayerInput>();
        input.actions = ScriptableObject.CreateInstance<InputActionAsset>();
        
        _playerGo.AddComponent<MeshRenderer>();
        _playerGo.AddComponent<PlayerItemsManager>();

        _player = _playerGo.AddComponent<Player>();
    }

    [TearDown]
    public void Cleanup()
    {
        Object.DestroyImmediate(_playerGo);
    }

    [Test]
    public void IsMoving_ReturnsFalse_WhenNoInput()
    {
        Assert.IsFalse(_player.IsMoving());
    }
    
    [Test]
    public void ApplyKnockback_SetsKnockbackVelocity()
    {
        Vector3 dir = Vector3.right;

        _player.ApplyKnockback(dir, 10f);

        // Use reflection since _knockbackVelocity is private
        var field = typeof(Player)
            .GetField("_knockbackVelocity", 
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

        Vector3 value = (Vector3)field.GetValue(_player);

        Assert.AreEqual(dir.normalized * 10f, value);
    }
}