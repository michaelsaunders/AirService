using System;

namespace AirService.Data.Contracts
{
    public interface ICacheProvider
    {
        T Get<T>(string key) where T : class;

        DateTime Cache(string key,
                       object objectToCache,
                       string dependantKey = null);

        void InvalidateCachesByMatchingKeyPattern(string keyPattern);

        DateTime? GetLastModifiedDate(string key);
    }
}