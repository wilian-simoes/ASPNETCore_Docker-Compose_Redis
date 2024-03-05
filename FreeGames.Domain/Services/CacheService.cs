using FreeGames.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace FreeGames.Domain.Services
{
    public class CacheService(IDistributedCache cache, ILogger<CacheService> logger) : ICacheService
    {
        private readonly IDistributedCache _cache = cache;

        public async Task<string> GetCache(string cacheKey)
        {
            try
            {
                logger.LogInformation("Buscando no cache.");
                return await _cache.GetStringAsync(cacheKey);
            }
            catch (Exception ex)
            {

                logger.LogError($"Erro ao acessar o cache. Exception: {ex.Message}");
                return null;
            }
        }

        public async Task SetCache(string cacheKey, string value)
        {
            try
            {
                DistributedCacheEntryOptions opcoesCache = new();
                opcoesCache.SetSlidingExpiration(TimeSpan.FromDays(1));

                logger.LogInformation("Gravando no cache.");
                await _cache.SetStringAsync(cacheKey, value, opcoesCache);
            }
            catch (Exception ex)
            {

                logger.LogError($"Erro ao gravar no cache. Exception: {ex.Message}");
            }
        }
    }
}