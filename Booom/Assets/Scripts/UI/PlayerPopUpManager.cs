using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class PlayerPopUpManager : MonoBehaviour
{
    [SerializeField] 
    private TextMeshProUGUI killStreakText;
    
    [SerializeField] 
    private TextMeshProUGUI killStreakInfoText;
    
    [SerializeField] 
    private TextMeshProUGUI killStreakTextFr;
    
    [SerializeField] 
    private TextMeshProUGUI killStreakInfoTextFr;
    
    [SerializeField]
    private Image itemTextPopUpBackground;
    
    [SerializeField]
    private LocalizeStringEvent itemTextPopUpLocalized;

    [SerializeField]
    private RawImage itemIconPrefab;
    
    [SerializeField]
    private Transform itemIconsContainer;
    
    private Dictionary<ItemType, RawImage> _activeIcons = new();
    private float _popUpDuration = GameConstants.POPUP_DURATION;
    private Coroutine _popUpCoroutine;
    private Coroutine _killStreakCoroutine;
    
    private void Start()
    {
#if !UNITY_EDITOR
        _popUpDuration = GameManager.Instance.RuntimeConfig.PopUpDuration;
#endif
    }

    public void DisplayKillStreak(int rangeBonus)
    {
        if(_killStreakCoroutine != null)
            StopCoroutine(_killStreakCoroutine);
        _killStreakCoroutine = StartCoroutine(DisplayKillStreakCoroutine(rangeBonus));
    }
    
    private IEnumerator DisplayKillStreakCoroutine(int rangeBonus)
    {
        if (LobbyManager.TokebaqueIcitte)
        {
            killStreakTextFr.gameObject.SetActive(true);
            killStreakInfoTextFr.gameObject.SetActive(true);
        }
        else
        {
            killStreakText.gameObject.SetActive(true);
            killStreakInfoText.gameObject.SetActive(true);
        }
        yield return new WaitForSeconds(3f);
        if (LobbyManager.TokebaqueIcitte)
        {
            killStreakTextFr.gameObject.SetActive(false);
            killStreakInfoTextFr.gameObject.SetActive(false);
        }
        else
        {
            killStreakText.gameObject.SetActive(false);
            killStreakInfoText.gameObject.SetActive(false);
        }
    }
    
    public void DisplayPopUp(in ItemType itemType, in Sprite iconSprite)
    {
        itemTextPopUpLocalized.StringReference.TableReference = "UI_Text";
        itemTextPopUpLocalized.StringReference.TableEntryReference = itemType.ToString();
        itemTextPopUpLocalized.RefreshString();
        
        AddIcon(itemType, iconSprite);
        
        if (_popUpCoroutine != null)
            StopCoroutine(_popUpCoroutine);
        _popUpCoroutine = StartCoroutine(DisplayPopUpCoroutine());
    }
    
    private IEnumerator DisplayPopUpCoroutine()
    {
        itemTextPopUpBackground.gameObject.SetActive(true);
        yield return new WaitForSeconds(_popUpDuration);
        itemTextPopUpBackground.gameObject.SetActive(false);
    }
    
    private void AddIcon(in ItemType itemType, in Sprite sprite)
    {
        if (_activeIcons.ContainsKey(itemType))
            return;

        var icon = Instantiate(itemIconPrefab, itemIconsContainer);
        icon.GetComponentInChildren<Image>().sprite = sprite;

        _activeIcons[itemType] = icon;
    }
    
    public void RemoveItemPopUp(in ItemType itemType)
    {
        if (!_activeIcons.TryGetValue(itemType, out var icon))
            return;

        _activeIcons.Remove(itemType);
        Destroy(icon.gameObject);
    }
}
