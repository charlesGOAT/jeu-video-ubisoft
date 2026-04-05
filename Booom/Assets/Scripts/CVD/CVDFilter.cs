using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SOG.CVDFilter
{
    [RequireComponent(typeof(Volume))]
    public class CVDFilter : MonoBehaviour
    {
        public static CVDFilter Instance { get; private set; }
        
        [SerializeField] private CVDProfilesSO profiles;
        [SerializeField] private VisionTypeNames currentType;

        // Subfolder path inside Resources (e.g. Resources/Profiles/)
        private const string ProfilesResourcePath = "Profiles/";

        public VisionTypeInfo SelectedVisionType { get; private set; }

        private Volume postProcessVolume;

        private CVDButton _cvdButton;

        // -------------------- Unity Callbacks --------------------
        private void Reset()
        {
            Setup();
            ChangeProfile();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(Instance.gameObject);
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Setup();
        }

        private void Start()
        {
            _cvdButton = FindFirstObjectByType<CVDButton>(FindObjectsInactive.Include);
            if (_cvdButton != null)
            {
                _cvdButton.OnChangeFilterCalled += SetVisionType;
            }
            ChangeProfile();
        }

        private void OnDestroy()
        {
            if (_cvdButton != null)
                _cvdButton.OnChangeFilterCalled -= SetVisionType;
        }

        // -------------------- Setup --------------------
        private void Setup()
        {
            if (profiles == null)
            {
                Debug.LogError("[CVDFilter]: No CVDProfiles assigned. Drag the asset into the Inspector!");
                return;
            }

            postProcessVolume = GetComponent<Volume>();

            if (postProcessVolume == null)
            {
                Debug.LogError("[CVDFilter]: No Volume component found on this GameObject.");
                return;
            }

            postProcessVolume.isGlobal = true;
        }

        // -------------------- Runtime Methods --------------------
        private void ChangeProfile()
        {
            if (profiles == null || profiles.VisionTypes == null || profiles.VisionTypes.Count == 0)
            {
                Debug.LogError("[CVDFilter]: Cannot change profile. VisionTypes list is empty or missing.");
                return;
            }

            if (postProcessVolume == null)
            {
                Debug.LogError("[CVDFilter]: postProcessVolume is null. Make sure Setup() ran successfully.");
                return;
            }

            int index = Mathf.Clamp((int)currentType, 0, profiles.VisionTypes.Count - 1);
            SelectedVisionType = profiles.VisionTypes[index];

            // If the profile reference is missing, try loading it from Resources/Profiles/
            VolumeProfile resolvedProfile = SelectedVisionType.profile;

            if (resolvedProfile == null)
            {
                string resourcePath = ProfilesResourcePath + SelectedVisionType.typeName.ToString();
                resolvedProfile = Resources.Load<VolumeProfile>(resourcePath);

                if (resolvedProfile == null)
                {
                    Debug.LogWarning($"[CVDFilter]: Could not find VolumeProfile for '{SelectedVisionType.typeName}' " +
                                     $"at Resources/{resourcePath}. Make sure the asset exists there.");
                    return;
                }

                Debug.Log($"[CVDFilter]: Loaded profile '{SelectedVisionType.typeName}' from Resources.");
            }

            postProcessVolume.profile = resolvedProfile;
        }

        /// <summary>
        /// Switch vision type at runtime.
        /// </summary>
        public void SetVisionType(int index)
        {
            currentType = profiles.VisionTypes[index].typeName;
            ChangeProfile();
        }
    }

    // -------------------- Vision Types --------------------
    public enum VisionTypeNames
    {
        Normal,
        Protanopia,
        Protanomaly,
        Deuteranopia,
        Deuteranomaly,
        Tritanopia,
        Tritanomaly,
        Achromatopsia,
        Achromatomaly
    }

    [System.Serializable]
    public struct VisionTypeInfo
    {
        public VisionTypeNames typeName;
        public string description;
        public VolumeProfile profile;
        public Texture2D previewImage;
    }
}