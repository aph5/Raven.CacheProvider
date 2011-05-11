using System;

namespace Raven.CacheProvider
{
    public class CacheItem
    {
        public  CacheItem()
        {
        }

        public CacheItem(string id, DateTime expiry)
        {
            Id = id;
            Expiry = expiry;
        }

        public String Id { get; set; }
        public DateTime Expiry { get; set; }
    }
}