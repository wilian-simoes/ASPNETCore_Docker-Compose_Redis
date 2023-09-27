using FreeGames.Domain.Interfaces;
using FreeGames.Domain.Models;
using Microsoft.Extensions.Logging;

namespace FreeGames.Domain.Services
{
    public class EpicGamesService : IEpicGamesService
    {
        private readonly HttpClient _client;
        private readonly ILogger<EpicGamesService> _logger;
        private static readonly List<FreeGamesPromotions.Element> jogosGratisResultadosMemoryCache = new();

        public EpicGamesService(HttpClient client, ILogger<EpicGamesService> logger)
        {
            _client = client;
            _logger = logger;
        }

        private async Task<FreeGamesPromotions> GetFreeGamesPromotions()
        {
            var response = await _client.GetAsync("https://store-site-backend-static-ipv4.ak.epicgames.com/freeGamesPromotions?locale=pt-BR&country=BR&allowCountries=BR");

            _logger.LogInformation($"Requisicao na Epic Games. Resposta: {response.StatusCode}");
            
            if (!response.IsSuccessStatusCode)
                throw new Exception("Erro ao conectar-se a API da Epic Games.");

            var jsonResponse = await response.Content.ReadAsStringAsync();
            return System.Text.Json.JsonSerializer.Deserialize<FreeGamesPromotions>(jsonResponse);
        }

        private async Task<List<FreeGamesPromotions.Element>> ListarJogosGratis()
        {
            var jogosGratisResultados = new List<FreeGamesPromotions.Element>();

            if (jogosGratisResultadosMemoryCache.Count == 0)
            {
                var novosJogos = await GetFreeGamesPromotions();

                // Filtra os jogos que são gratuitos para jogar
                var jogosFiltrados = novosJogos.data.Catalog.searchStore.elements.Where(e => e.promotions != null && e.offerType == "BASE_GAME");

                // Filtra os jogos que estão gratuitos agora
                var jogosGratisAgora = jogosFiltrados.Where(jogo => jogo.promotions.promotionalOffers != null && jogo.promotions.promotionalOffers.Count > 0 &&
                    jogo.promotions.promotionalOffers[0].promotionalOffers[0].startDate.Date <= DateTime.Now.Date &&
                    jogo.promotions.promotionalOffers[0].promotionalOffers[0].endDate.Date >= DateTime.Now.Date);

                // Filtra os jogos que serão gratuitos no futuro
                var jogosGratisFuturos = jogosFiltrados.Where(jogo => jogo.promotions.promotionalOffers != null && jogo.promotions.upcomingPromotionalOffers != null &&
                    jogo.promotions.upcomingPromotionalOffers.Count > 0 &&
                    jogo.promotions.upcomingPromotionalOffers[0].promotionalOffers[0].endDate.Date >= DateTime.Now.Date);

                // Adiciona os jogos gratuitos agora à lista de resultados
                jogosGratisResultadosMemoryCache.AddRange(jogosGratisAgora);

                // Adiciona a data em que o jogo estará disponível
                jogosGratisFuturos.ToList().ForEach(jogo => jogo.title += $" (Disponível em {jogo.promotions.upcomingPromotionalOffers[0].promotionalOffers[0].startDate:dd/MM/yyyy})");

                // Adiciona os jogos gratuitos no futuro à lista de resultados, com a data de lançamento
                jogosGratisResultadosMemoryCache.AddRange(jogosGratisFuturos);
            }

            jogosGratisResultados = jogosGratisResultadosMemoryCache;

            _logger.LogInformation("Filtrou os jogos gratis.");

            return jogosGratisResultados;
        }

        public async Task<object> GetFreeGames()
        {
            var response = await ListarJogosGratis();

            if (response == null)
            {
                string mensagem = "Nao foi possivel obter os jogos gratis.";
                _logger.LogInformation(mensagem);
                throw new Exception(mensagem);
            }

            return response.Select(r => new
            {
                Nome = r.title,
                Descricao = r.description,
                URL = $"https://store.epicgames.com/pt-BR/p/{r.catalogNs.mappings[0].pageSlug}",
                Imagem = r.keyImages[0].url
            });
        }
    }
}