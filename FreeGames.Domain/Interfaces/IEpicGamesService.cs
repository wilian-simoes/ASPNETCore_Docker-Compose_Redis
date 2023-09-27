namespace FreeGames.Domain.Interfaces
{
    public interface IEpicGamesService
    {
        Task<object> GetFreeGames();
    }
}