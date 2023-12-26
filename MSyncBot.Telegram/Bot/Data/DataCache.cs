namespace MSyncBot.Telegram.Bot.Data;

public class CacheItem<T>
{
    public T Data { get; set; }
    public DateTime ExpirationTime { get; set; }
}

public class DataCache<T>
{
    private readonly Dictionary<string, CacheItem<T>> _cache = new();

    public void AddOrUpdate(string key, T data, TimeSpan expirationTime)
    {
        var expiration = DateTime.Now.Add(expirationTime);
        var cacheItem = new CacheItem<T> { Data = data, ExpirationTime = expiration };

        _cache[key] = cacheItem;
    }

    public T Get(string key)
    {
        if (!_cache.TryGetValue(key, out CacheItem<T> cacheItem))
            return default(T);
        
        if (cacheItem.ExpirationTime > DateTime.Now)
        {
            return cacheItem.Data;
        }
        
        _cache.Remove(key);
        return default(T);
    }

    public void Remove(string key) => _cache.Remove(key);

    public void Clear() => _cache.Clear();
}