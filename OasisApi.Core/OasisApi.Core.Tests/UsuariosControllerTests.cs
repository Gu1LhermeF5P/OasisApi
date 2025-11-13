using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using OasisApi.Core.Dtos;
using OasisApi.Core.Helpers;
using Xunit; 

namespace OasisApi.Core.Tests
{
   
    public class UsuariosControllerTests : IClassFixture<OasisApiApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonOptions;

        // Este é o CONSTRUTOR (O nome está correto)
        public UsuariosControllerTests(OasisApiApplicationFactory factory)
        {
            _client = factory.CreateClient();
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        // --- Teste 1: Buscar um usuário que existe ---
        [Fact]
        public async Task GetUsuarioPorId_DeveRetornar200OK_QuandoUsuarioExiste()
        {
            // Arrange
            var userId = 1;

            // Act
            var response = await _client.GetAsync($"/api/v1/usuarios/{userId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var jsonString = await response.Content.ReadAsStringAsync();
            var usuario = JsonSerializer.Deserialize<UsuarioDto>(jsonString, _jsonOptions);
            Assert.NotNull(usuario);
            Assert.Equal(userId, usuario.UsuarioId);
        }

        // --- Teste 2: Buscar um usuário que NÃO existe ---
        [Fact]
        public async Task GetUsuarioPorId_DeveRetornar404NotFound_QuandoUsuarioNaoExiste()
        {
            // Arrange
            var userId = 999;

            // Act
            var response = await _client.GetAsync($"/api/v1/usuarios/{userId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // --- Teste 3: Buscar a lista de usuários (Paginação e HATEOAS) ---
        [Fact]
        public async Task GetUsuarios_DeveRetornar200OK_EListaPaginadaCorreta()
        {
            // Arrange
            var pageNumber = 1;
            var pageSize = 10;

            // Act
            var response = await _client.GetAsync($"/api/v1/usuarios?pageNumber={pageNumber}&pageSize={pageSize}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var jsonString = await response.Content.ReadAsStringAsync();
            var pagedResult = JsonSerializer.Deserialize<PagedResult<UsuarioDto>>(jsonString, _jsonOptions);
            Assert.NotNull(pagedResult);
            Assert.Equal(2, pagedResult.TotalRecords); 
            Assert.Contains(pagedResult.Links, l => l.Rel == "self"); 
        }
    }
}