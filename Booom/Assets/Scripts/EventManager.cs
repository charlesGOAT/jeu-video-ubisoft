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
    
    private readonly Dictionary<Tuple<int, int>, Tuple<BombEnum, bool>> _bombEventsDict = new();  // <minutes, seconds> -> <bomb type, hasEventHappened>

    private void Awake()
    {
        foreach (var pair in bombEvents)
        {
            _bombEventsDict.TryAdd(new Tuple<int, int>(pair.minutes, pair.seconds), new Tuple<BombEnum, bool>(pair.value, false));
        }
        
        if (_bombEventsDict.TryGetValue(new Tuple<int, int>(0, 0), out Tuple<BombEnum, bool> defaultBombType))
            CurrentBombType = defaultBombType.Item1;
    }
    private void Update()
    {
        var timeTuple = new Tuple<int, int>(GameManager.Instance.CurrentMinutes, GameManager.Instance.CurrentSeconds);
        if (_bombEventsDict.TryGetValue(timeTuple, out Tuple<BombEnum, bool> bombType) && !bombType.Item2)
        {
            CurrentBombType = bombType.Item1;
            _bombEventsDict[timeTuple] = new Tuple<BombEnum, bool>(CurrentBombType, true);
            GameManager.Instance.GameUIManager.RefreshBombType(bombType.Item1.ToString().AddSpacesBeforeCaps());
            GameManager.Instance.GameUIManager.DisplayEventPanel(CurrentBombType.ToString().AddSpacesBeforeCaps());
            SoundManager.Instance.OnBombEvent();
        }
    }
}
