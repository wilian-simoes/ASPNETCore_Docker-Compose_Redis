using FreeGames.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace FreeGames.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EpicGamesController : ControllerBase
    {
        private readonly IEpicGamesService _epicGamesService;

        public EpicGamesController(IEpicGamesService epicGamesService)
        {
            _epicGamesService = epicGamesService;
        }

        /// <summary>
        /// Obtém os jogos grátis da semana na epic games.
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetFreeGames")]
        public async Task<IActionResult> GetFreeGames([FromServices] IDistributedCache cache, [FromServices] ILogger<EpicGamesController> logger)
        {
            try
            {
                string cacheKey = "EpicGames";
                string jogosGratisCacheRedis = cache.GetString(cacheKey);
                logger.LogInformation("Buscou no cache.");

                if(jogosGratisCacheRedis == null)
                {
                    var response = await _epicGamesService.GetFreeGames();

                    DistributedCacheEntryOptions opcoesCache = new();
                    opcoesCache.SetSlidingExpiration(TimeSpan.FromDays(1));

                    jogosGratisCacheRedis = System.Text.Json.JsonSerializer.Serialize(response);
                    cache.SetString(cacheKey,jogosGratisCacheRedis);
                    logger.LogInformation("Salvou no cache.");
                }
                
                return Ok(jogosGratisCacheRedis);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }
    }
}