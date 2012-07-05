using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Runtime.Caching;
using AirService.Data.Contracts;

namespace AirService.Services
{ 
    /// <summary>
    /// If there is IO contention, modifying/deleting cache dependancy files may throw errors. 
    /// In this case next request will try to cache. 
    /// It should be cheaper not to use lock/semaphore etc. 
    /// </summary>
    public class MemoryCacheProvider : ICacheProvider
    {
        private readonly MemoryCache _cache; 
        private readonly string _cacheDirectory;
        
        public MemoryCacheProvider()
        {
            this._cache = MemoryCache.Default;
            this._cacheDirectory = ConfigurationManager.AppSettings["MemoryCacheCacheDirectory"];
        }

        #region ICacheProvider Members

        public void InvalidateCachesByMatchingKeyPattern(string keyPattern)
        {
            string[] files = Directory.GetFiles(this._cacheDirectory,
                                                keyPattern,
                                                SearchOption.TopDirectoryOnly);
            foreach (string file in files)
            {
                this.UpdateCacheFile(file);
            } 
        }


        public DateTime? GetLastModifiedDate(string key)
        {
            DateTime? date = null;
            try
            {
                string path = Path.Combine(this._cacheDirectory,
                                           key);
                if (File.Exists(path))
                {
                    date = File.GetLastWriteTimeUtc(path);
                }
            }
            catch
            {
            }

            return date;
        }

        public T Get<T>(string key) where T : class
        {
            T cachedObject = null;
            try
            {
                if (this._cache.Contains(key))
                {
                    cachedObject = (T) this._cache[key];
                }
            }
            catch
            {
            }

            return cachedObject;
        }

        public DateTime Cache(string key,
                              object objectToCache,
                              string dependantKey = null)
        {
            try
            {
                // first, create cache file if not exists already.
                string cacheFilePath = Path.Combine(this._cacheDirectory,
                                                    key);
                bool updateCacheFile = true;
                if(dependantKey == null)
                {
                    if (File.Exists(cacheFilePath) && File.Exists(cacheFilePath + ".tmp"))
                    {
                        // Child cache was created first and nothing was changed since
                        updateCacheFile = false;
                        File.Delete(cacheFilePath + ".tmp");
                    }
                }

                if (updateCacheFile)
                {
                    File.WriteAllText(cacheFilePath,
                                  DateTime.Now.ToString());
                }

                var filePaths = new List<string> {cacheFilePath}; 
                // get the file modified time from the cache file or the file this cache depends on
                DateTime lastModifiedDateTime;
                if (dependantKey != null)
                {
                    string dependantFilePath = Path.Combine(this._cacheDirectory,
                                                            dependantKey);
                    // shouldn't touch the file if it already exists or will invalidate cache 
                    if (!File.Exists(dependantFilePath))
                    {
                        File.WriteAllText(dependantFilePath,
                                          DateTime.Now.ToString());
                        // create a temp file to indicate child cache was created first.
                        File.WriteAllText(dependantFilePath + ".tmp",
                                          DateTime.Now.ToString());
                    }

                    lastModifiedDateTime = File.GetLastWriteTimeUtc(dependantFilePath);
                    filePaths.Add(dependantFilePath); 
                }
                else
                {
                    lastModifiedDateTime = File.GetLastWriteTimeUtc(cacheFilePath);
                }

                var cacheItem = new CacheItem(key,
                                              objectToCache,
                                              null);
                var policy = new CacheItemPolicy();
                policy.ChangeMonitors.Add(new HostFileChangeMonitor(filePaths));
                this._cache.Set(cacheItem,
                                policy);
                return lastModifiedDateTime;
            }
            catch (Exception e)
            {
                // mostly due to the file access contention. Ignore cache and continue.
                Debug.WriteLine(e.Message);
                return DateTime.Now;
            }
        }

        #endregion

        private void UpdateCacheFile(string filePath)
        {
            int tried = 0;
            while (true)
            {    
                try
                {
                    if (!File.Exists(filePath))
                    {
                        break;
                    }

                    File.Delete(filePath);
                    return;
                }
                catch
                {
                    if (++tried > 100)
                    {
                        throw new ApplicationException("Unable to update cache file");
                    }
                }
            }
        }
    }
}