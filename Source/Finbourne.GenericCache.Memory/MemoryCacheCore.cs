using Finbourne.GenericCache.Core.Interface;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Finbourne.GenericCache.Memory
{
    public class MemoryCacheCore : IGenericCache
    {
        private readonly MemoryCacheConfig config;
        private readonly ILogger<MemoryCacheCore> logger;
        private readonly ConcurrentDictionary<string, object> _cacheList;
        private readonly LinkedList<string> _lruCache;
        private readonly object _lock = new();

        public MemoryCacheCore(MemoryCacheConfig config, ILogger<MemoryCacheCore> logger)
        {
            if (config.Capacity <= 0)
            {
                throw new ArgumentException("Capacity of cache must be greater than 0.", nameof(config.Capacity));
            }
            this.config = config;
            this.logger = logger;

            _cacheList = new ConcurrentDictionary<string, object>();
            _lruCache = new LinkedList<string>();
        }

        public MemoryCacheCore(MemoryCacheConfig config) : this(config, null)
        {
        }

        public MemoryCacheCore() : this(new MemoryCacheConfig() { Capacity = 100 }, null)
        {
        }

        public Task<bool> DeleteAsync<T>(string key)
        {
            var wasRemoved = _cacheList.TryRemove(key, out var removedValue);
            if (wasRemoved)
            {
                logger?.LogInformation($"Key {key} removed from cache.");
                lock (_lock)
                {
                    _lruCache.Remove(key);
                }
            }
            else
            {
                logger?.LogWarning($"Key {key} not found in cache.");
            }
            return Task.FromResult(wasRemoved);
        }

        public Task<bool> TryGetAsync<T>(string key, out T value)
        {
            if (_cacheList.TryGetValue(key, out var _value))
            {
                logger?.LogInformation($"Key {key} found in cache.");
                value = (T)_value;
                
                lock (_lock)
                {
                    _lruCache.Remove(key);
                    _lruCache.AddFirst(key);
                }
                return Task.FromResult(true);
            }
            logger?.LogInformation($"Key {key} not found in cache.");
            value = default;
            return Task.FromResult(false);
        }
        public Task SetAsync<T>(string key, T value)
        {
            lock (_lock)
            {
                if (_cacheList.Count >= config.Capacity && !_cacheList.ContainsKey(key))
                {
                    var oldestKey = _lruCache.Last?.Value;
                    logger?.LogInformation($"Cache capacity reached. Removing oldest key: ${oldestKey}.");
                    if (!string.IsNullOrEmpty(oldestKey))
                    {
                        _cacheList.TryRemove(oldestKey, out var oldValue);
                        logger?.LogInformation($"Oldest key {oldestKey} removed from cache.");
                        _lruCache.RemoveLast();                        
                    }
                }
                _cacheList.AddOrUpdate(key, value, (_, _) => value);                
                _lruCache.Remove(key);                
                _lruCache.AddFirst(key);
            }

            logger?.LogInformation($"Key {key} added to cache.");
            return Task.CompletedTask;
        }
    }
}