using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;


[Serializable]
public class KeyValuePair {
    public int minutes;
    public int seconds;
    public BombEnum value;
    
    public KeyValuePair(int mins, int secs, BombEnum type)
    {
        minutes = mins;
        seconds = secs;
        value = type;
    }
}

[Serializable]
public class KeyValuePairText {
    public int minutes;
    public int seconds;
    public string LocalizationKey;
    
    public KeyValuePairText(int mins, int secs, string key)
    {
        minutes = mins;
        seconds = secs;
        LocalizationKey = key;
    }
}

public class EventManager : MonoBehaviour
{
    public BombEnum CurrentBombType { get; private set; } = BombEnum.NormalBomb;

    [Tooltip("To have a default bomb type other than NormalBomb, add this bomb type with <0,0> key.")]
    [Header("<minutes, seconds> -> bomb type")]
    [SerializeField]
    private List<KeyValuePair> bombEvents = new ();
    
    [Header("<minutes, seconds> -> Text event text")]
    [SerializeField]
    private List<KeyValuePairText> textEvents = new ();
    
    private readonly Dictionary<Tuple<int, int>, Tuple<BombEnum, bool>> _bombEventsDict = new();  // <minutes, seconds> -> <bomb type, hasEventHappened>
    private readonly Dictionary<Tuple<int, int>, Tuple<string, bool>> _textEventsDict = new();  // <minutes, seconds> -> <string, hasEventHappened>

    public void Start()
    {
#if !UNITY_EDITOR
    SetUpConfigValues();
#endif
        foreach (var pair in bombEvents)
        {
            _bombEventsDict.TryAdd(new Tuple<int, int>(pair.minutes, pair.seconds), new Tuple<BombEnum, bool>(pair.value, false));
        }
        
        foreach (var pair in textEvents)
        {
            _textEventsDict.TryAdd(new Tuple<int, int>(pair.minutes, pair.seconds), new Tuple<string, bool>(pair.LocalizationKey, false));
        }
        
        if (_bombEventsDict.TryGetValue(new Tuple<int, int>(0, 0), out Tuple<BombEnum, bool> defaultBombType))
            CurrentBombType = defaultBombType.Item1;
    }
    
    private void Update()
    {
        var timeTuple = new Tuple<int, int>(GameManager.Instance.CurrentMinutes, GameManager.Instance.CurrentSeconds);
        if (ManageEvents(timeTuple)) return;
        ManageBombEvents(timeTuple);
    }

    private void SetUpConfigValues()
    {
        bombEvents = GameManager.Instance.RuntimeConfig.BombEvents;
        textEvents = GameManager.Instance.RuntimeConfig.TextEvents;
    }

    private void ManageBombEvents(in Tuple<int, int> timeTuple)
    {
        if (_bombEventsDict.TryGetValue(timeTuple, out Tuple<BombEnum, bool> bombType) && !bombType.Item2)
        {
            CurrentBombType = bombType.Item1;
            _bombEventsDict[timeTuple] = new Tuple<BombEnum, bool>(CurrentBombType, true);
            GameManager.Instance.GameUIManager.RefreshBombType(CurrentBombType.ToString().AddSpacesBeforeCaps());
            GameManager.Instance.GameUIManager.DisplayEventPanel();
            SoundManager.Instance.OnBombEvent();
        }
    }

    private bool ManageEvents(in Tuple<int, int> timeTuple)
    {
        if (_textEventsDict.TryGetValue(timeTuple, out Tuple<string, bool> value) && !value.Item2)
        {
            LocalizedString eventText = new LocalizedString
            {
                TableReference = "UI_Text",
                TableEntryReference = value.Item1
            };
            
            GameManager.Instance.GameUIManager.DisplayEventPanel(eventText);
            _textEventsDict[timeTuple] = new Tuple<string, bool>(value.Item1, true);
            return true;
        } 
        
        return false;
    }
}
