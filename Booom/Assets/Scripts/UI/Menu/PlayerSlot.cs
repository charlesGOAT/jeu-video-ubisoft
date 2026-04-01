using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class PlayerSlot : MonoBehaviour
{
    public LocalizedString playerLabel; // "Player {0}"
    public LocalizedString joinPrompt;
    public LocalizeStringEvent playerLabelLocalized;
    public Image coloredCharacter;
    public Image lockedImage;
}
