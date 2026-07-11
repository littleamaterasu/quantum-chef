using System;

namespace Framework.Storage
{
    [Serializable]
    public class BaseAccountData : IAccountData
    {
        public string AccountId { get; set; }
        public long LastUpdatedUnix { get; set; }

        public void Touch()
        {
            LastUpdatedUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }
}
