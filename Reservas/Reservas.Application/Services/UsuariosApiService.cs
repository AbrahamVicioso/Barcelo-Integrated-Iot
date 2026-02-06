using System.Text.Json;
using Microsoft.Extensions.Logging;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Services;

public class UsuariosApiService : IUsuariosApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UsuariosApiService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public UsuariosApiService(HttpClient httpClient, ILogger<UsuariosApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<HuespedeDto?> GetHuespedByUsuarioIdAsync(string usuarioId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Obteniendo huésped para usuarioId: {UsuarioId}", usuarioId);

            var response = await _httpClient.GetAsync($"/Huesped/user/{usuarioId}", cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Huésped no encontrado para usuarioId: {UsuarioId}", usuarioId);
                return null;
            }

            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            var huesped = JsonSerializer.Deserialize<HuespedeDto>(jsonResponse, _jsonOptions);

            if (huesped == null)
            {
                _logger.LogWarning("No se pudo deserializar el huésped para usuarioId: {UsuarioId}", usuarioId);
                return null;
            }

            _logger.LogInformation("Huésped encontrado: {HuespedId} para usuarioId: {UsuarioId}", huesped.HuespedId, usuarioId);
            return huesped;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error al consumir la API de Usuarios para obtener huésped con usuarioId: {UsuarioId}", usuarioId);
            throw new Exception($"Error al obtener el huésped desde la API de Usuarios: {ex.Message}", ex);
        }
    }
}
