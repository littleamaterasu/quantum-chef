using System;
using System.Collections.Generic;
using Framework.Storage;

namespace Gameplay.Scripts.Data
{
    [Serializable]
    public class UserData : BaseAccountData
    {
        public int coin = 0;
        public List<string> ownedToolIds = new List<string>();
        public int reachedLevel = 0;
        public List<int> levelStars = new List<int>();
    }
}
