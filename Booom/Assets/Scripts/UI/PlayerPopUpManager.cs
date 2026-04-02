using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPopUpManager : MonoBehaviour
{
    [SerializeField] 
    private TextMeshProUGUI killStreakText;
    
    [SerializeField] 
    private TextMeshProUGUI killStreakInfoText;
    
    [SerializeField]
    private Image itemTextPopUpBackground;
    
    [SerializeField]
    private TMP_Text itemTextPopUpText;

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
        killStreakText.gameObject.SetActive(true);
        killStreakInfoText.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        killStreakText.gameObject.SetActive(false);
        killStreakInfoText.gameObject.SetActive(false);
    }
    
    public void DisplayPopUp(in ItemType itemType, in Sprite iconSprite)
    {
        itemTextPopUpText.text = itemType.ToString().AddSpacesBeforeCaps().ToUpper();
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
