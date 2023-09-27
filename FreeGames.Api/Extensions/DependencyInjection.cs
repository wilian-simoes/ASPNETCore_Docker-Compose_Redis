using FreeGames.Domain.Interfaces;
using FreeGames.Domain.Services;

namespace FreeGames.Api.Extensions
{
    public static class DependencyInjection
    {
        public static void RegisterServices(this IServiceCollection services, IConfiguration configuration) 
        {
            services.AddHttpClient<IEpicGamesService, EpicGamesService>();
        }
    }
}