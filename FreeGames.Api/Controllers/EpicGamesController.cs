using FreeGames.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FreeGames.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EpicGamesController : ControllerBase
    {
        private const string CACHE_KEY = "EpicGames";

        private readonly ILogger<EpicGamesController> _logger;
        private readonly IEpicGamesService _epicGamesService;
        private readonly ICacheService _cacheService;

        public EpicGamesController(ILogger<EpicGamesController> logger, IEpicGamesService epicGamesService, ICacheService cacheService)
        {
            _logger = logger;
            _epicGamesService = epicGamesService;
            _cacheService = cacheService;
        }

        /// <summary>
        /// Obtém os jogos grátis da semana na epic games.
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetFreeGames")]
        public async Task<IActionResult> GetFreeGames()
        {
            try
            {
                _logger.LogInformation("Iniciou o endpoint GetFreeGames.");

                string jogosGratisCacheRedis = await _cacheService.GetCache(CACHE_KEY);

                if (jogosGratisCacheRedis == null)
                {
                    _logger.LogInformation("Iniciando consulta na Epic Games");
                    var response = await _epicGamesService.GetFreeGames();
                    jogosGratisCacheRedis = System.Text.Json.JsonSerializer.Serialize(response);

                    await _cacheService.SetCache(CACHE_KEY, jogosGratisCacheRedis);
                }

                _logger.LogInformation("Endpoint GetFreeGames finalizado com sucesso.");
                return Ok(jogosGratisCacheRedis);
            }
            catch (Exception ex)
            {

                _logger.LogError($"Erro. Exception: {ex.Message}");
                throw new Exception("Ocorreu um erro inesperado.");
            }
        }
    }
}