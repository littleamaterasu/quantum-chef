using System;
using Gameplay.Scripts.Controller;
using Gameplay.Scripts.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Scripts.UI
{
    public class LevelCard : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text levelNameText;
        [SerializeField] private Image cardImage;
        [SerializeField] private Button cardButton;

        [Header("Sprites")]
        [SerializeField] private Sprite enabledSprite;
        [SerializeField] private Sprite disabledSprite;

        [Header("Star Icons (Optional)")]
        [SerializeField] private Image[] starImages;
        [SerializeField] private Sprite starFilledSprite;
        [SerializeField] private Sprite starEmptySprite;

        private int levelIndex;
        private LevelDataAsset levelDataAsset;
        private Action<int, LevelDataAsset> onClickAction;

        public void Setup(int index, LevelDataAsset dataAsset, bool isUnlocked, Action<int, LevelDataAsset> onClick)
        {
            this.levelIndex = index;
            this.levelDataAsset = dataAsset;
            this.onClickAction = onClick;

            // 1. Display Level Name
            if (levelNameText != null)
            {
                string nameStr = dataAsset != null && !string.IsNullOrEmpty(dataAsset.name)
                    ? dataAsset.name
                    : $"Level {index + 1}";
                levelNameText.text = nameStr;
            }

            // 2. Display Enabled / Disabled State
            if (isUnlocked)
            {
                if (cardImage != null && enabledSprite != null)
                {
                    cardImage.sprite = enabledSprite;
                }

                if (cardButton != null)
                {
                    cardButton.interactable = true;
                    cardButton.onClick.RemoveAllListeners();
                    cardButton.onClick.AddListener(OnCardClicked);
                }
            }
            else
            {
                if (cardImage != null && disabledSprite != null)
                {
                    cardImage.sprite = disabledSprite;
                }

                if (cardButton != null)
                {
                    cardButton.interactable = false;
                    cardButton.onClick.RemoveAllListeners();
                }
            }

            // 3. Display Stars Earned
            UpdateStarsDisplay(isUnlocked);
        }

        private void UpdateStarsDisplay(bool isUnlocked)
        {
            if (starImages == null || starImages.Length == 0) return;

            int starsEarned = isUnlocked ? UserDataManager.Instance.GetLevelStars(levelIndex) : 0;

            for (int i = 0; i < starImages.Length; i++)
            {
                if (starImages[i] == null) continue;

                bool filled = (i < starsEarned);
                if (starFilledSprite != null && starEmptySprite != null)
                {
                    starImages[i].sprite = filled ? starFilledSprite : starEmptySprite;
                }
                else
                {
                    starImages[i].gameObject.SetActive(filled);
                }
            }
        }

        private void OnCardClicked()
        {
            onClickAction?.Invoke(levelIndex, levelDataAsset);
        }
    }
}
