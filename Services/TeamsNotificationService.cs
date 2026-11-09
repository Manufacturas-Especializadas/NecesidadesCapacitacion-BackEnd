using Microsoft.Extensions.Options;
using NecesidadesCapacitacion.Data;
using System.Net.Http.Json;

namespace NecesidadesCapacitacion.Services
{
    public class TeamsNotificationService
    {
        public const string HttpClientName = "TeamsNotifications";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TeamsNotificationsSettings _settings;
        private readonly ILogger<TeamsNotificationService> _logger;

        public TeamsNotificationService(
            IHttpClientFactory httpClientFactory,
            IOptions<TeamsNotificationsSettings> options,
            ILogger<TeamsNotificationService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _settings = options.Value;
            _logger = logger;
        }

        public bool Enabled => _settings.Enabled;

        public async Task SendAsync(int trainingId, string text)
        {
            if (!Enabled)
            {
                return;
            }

            if (!Uri.TryCreate(_settings.WebhookUrl, UriKind.Absolute, out var webhookUri)
                || webhookUri.Scheme != Uri.UriSchemeHttps
                || !string.IsNullOrEmpty(webhookUri.UserInfo))
            {
                _logger.LogError(
                    "Teams no enviado para el folio {TrainingId}: configure un webhook HTTPS válido.",
                    trainingId);
                return;
            }

            try
            {
                using var client = _httpClientFactory.CreateClient(HttpClientName);
                using var response = await client.PostAsJsonAsync(webhookUri, new { text });

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Power Automate rechazó la notificación del folio {TrainingId}. Estado HTTP: {StatusCode}.",
                        trainingId, (int)response.StatusCode);
                    return;
                }

                _logger.LogInformation(
                    "Power Automate aceptó la notificación del folio {TrainingId}. Estado HTTP: {StatusCode}.",
                    trainingId, (int)response.StatusCode);
            }
            catch (OperationCanceledException)
            {
                _logger.LogError(
                    "Tiempo de espera de 10 segundos agotado al notificar el folio {TrainingId} a Teams. No se reintentará el envío.",
                    trainingId);
            }
            catch (HttpRequestException ex)
            {
                // No registrar la excepción ni su mensaje: pueden contener la URL firmada.
                _logger.LogError(
                    "Error HTTP o de conexión al notificar el folio {TrainingId} a Teams. Tipo: {HttpRequestError}. Estado HTTP: {StatusCode}.",
                    trainingId, ex.HttpRequestError, (int?)ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Error al notificar el folio {TrainingId} a Teams. Tipo: {ErrorType}.",
                    trainingId, ex.GetType().Name);
            }
        }
    }
}
