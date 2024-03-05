namespace FreeGames.Domain.Interfaces
{
    public interface ICacheService
    {
        Task<string> GetCache(string cacheKey);
        Task SetCache(string cacheKey, string value);
    }
}