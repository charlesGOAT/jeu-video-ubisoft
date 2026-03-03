using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class KeyValuePair {
    public int minutes;
    public int seconds;
    public BombEnum value;
}
public class EventManager : MonoBehaviour
{
    public BombEnum CurrentBombType { get; private set; } = BombEnum.NormalBomb;

    [Tooltip("To have a default bomb type other than NormalBomb, add this bomb type with <0,0> key.")]
    [Header("<minutes, seconds> -> bomb type")]
    [SerializeField]
    private List<KeyValuePair> bombEvents = new ();
    
    private readonly Dictionary<Tuple<int,int>, BombEnum> _bombEventsDict = new();  // <minutes, seconds> -> bomb type

    private void Awake()
    {
        foreach (var pair in bombEvents)
        {
            _bombEventsDict.TryAdd(new Tuple<int, int>(pair.minutes, pair.seconds), pair.value);
        }
        
        if (_bombEventsDict.TryGetValue(new Tuple<int, int>(0, 0), out BombEnum defaultBombType))
            CurrentBombType = defaultBombType;
    }
    private void Update()
    {
        if (_bombEventsDict.TryGetValue(
                new Tuple<int, int>(GameManager.Instance.CurrentMinutes, GameManager.Instance.CurrentSeconds),
                out BombEnum bombType))
        {
            CurrentBombType = bombType;
            GameManager.Instance.GameUIManager.RefreshBombType(bombType.ToString().AddSpacesBeforeCaps());
            GameManager.Instance.GameUIManager.DisplayEventPanel(CurrentBombType.ToString().AddSpacesBeforeCaps());

        }
    }
}
