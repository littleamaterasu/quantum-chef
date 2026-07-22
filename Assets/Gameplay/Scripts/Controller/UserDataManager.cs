using System;
using System.Collections.Generic;
using Framework.Storage;
using Gameplay.Scripts.Data;
using UnityEngine;

namespace Gameplay.Scripts.Controller
{
    [Serializable]
    public struct GameFinishResult
    {
        public int levelIndex;
        public int servedCount;
        public int totalCustomers;
        public float servedPercentage;
        public int starsEarned;
        public int coinsEarned;
        public int totalCoins;
    }

    public class UserDataManager : Singleton<UserDataManager>
    {
        private UserData data;

        public UserData Data
        {
            get
            {
                if (data == null)
                {
                    data = AccountManager.LoadOrCreate<UserData>();
                }
                return data;
            }
        }

        public void Save()
        {
            if (data != null)
            {
                data.Touch();
                AccountManager.Save(data);
            }
        }

        #region Coins

        public int Coin => Data.coin;

        public void AddCoins(int amount)
        {
            if (amount <= 0) return;
            Data.coin += amount;
            Save();
        }

        public bool UseCoins(int amount)
        {
            if (amount <= 0) return true;
            if (Data.coin < amount) return false;

            Data.coin -= amount;
            Save();
            return true;
        }

        #endregion

        #region Tools

        public IReadOnlyList<string> OwnedToolIds => Data.ownedToolIds;

        public bool HasTool(string toolId)
        {
            if (string.IsNullOrEmpty(toolId) || Data.ownedToolIds == null) return false;
            return Data.ownedToolIds.Contains(toolId);
        }

        public void AddTool(string toolId)
        {
            if (string.IsNullOrEmpty(toolId)) return;
            if (Data.ownedToolIds == null) Data.ownedToolIds = new List<string>();

            if (!Data.ownedToolIds.Contains(toolId))
            {
                Data.ownedToolIds.Add(toolId);
                Save();
            }
        }

        #endregion

        #region Level Progression & Stars

        public int ReachedLevel => Data.reachedLevel;

        public int GetLevelStars(int levelIndex)
        {
            if (levelIndex < 0 || Data.levelStars == null || levelIndex >= Data.levelStars.Count)
                return 0;

            return Data.levelStars[levelIndex];
        }

        /// <summary>
        /// Calculates star rating based on customer service percentage:
        /// &lt; 25% : 0 stars
        /// &lt; 50% : 1 star
        /// &lt; 75% : 2 stars
        /// &gt;= 75% : 3 stars
        /// </summary>
        public static int CalculateStars(float servedPercentage)
        {
            if (servedPercentage < 25f)
                return 0;
            if (servedPercentage < 50f)
                return 1;
            if (servedPercentage < 75f)
                return 2;
            return 3;
        }

        /// <summary>
        /// Finishes the level, calculates star rating based on served customer percentage,
        /// awards 25 coins per star earned, updates progression, and saves user data.
        /// </summary>
        public GameFinishResult FinishGame(int levelIndex, int servedCount, int totalCustomers)
        {
            float percentage = totalCustomers > 0 ? ((float)servedCount / totalCustomers) * 100f : 0f;
            int starsEarned = CalculateStars(percentage);
            int coinsEarned = starsEarned * 25;

            // Update coins
            Data.coin += coinsEarned;

            // Ensure levelStars list is large enough
            if (Data.levelStars == null)
            {
                Data.levelStars = new List<int>();
            }

            while (Data.levelStars.Count <= levelIndex)
            {
                Data.levelStars.Add(0);
            }

            // Save best stars achieved for this level
            Data.levelStars[levelIndex] = Math.Max(Data.levelStars[levelIndex], starsEarned);

            // Update reached level if needed
            if (levelIndex >= Data.reachedLevel)
            {
                Data.reachedLevel = levelIndex + 1;
            }

            Save();

            Debug.Log($"[UserDataManager] FinishGame Level {levelIndex}: Served {servedCount}/{totalCustomers} ({percentage:F1}%) -> Stars: {starsEarned}, Coins Earned: {coinsEarned}, Total Coins: {Data.coin}");

            return new GameFinishResult
            {
                levelIndex = levelIndex,
                servedCount = servedCount,
                totalCustomers = totalCustomers,
                servedPercentage = percentage,
                starsEarned = starsEarned,
                coinsEarned = coinsEarned,
                totalCoins = Data.coin
            };
        }

        #endregion
    }
}
