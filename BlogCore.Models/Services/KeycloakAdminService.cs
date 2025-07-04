using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BlogCore.Models.Services;
using BlogCore.Models.ViewModels;
using Microsoft.Extensions.Configuration;

public class KeycloakAdminService : IKeycloakAdminService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private string _accessToken;

    public KeycloakAdminService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<string> CreateUserAsync(RegisterUsuarioViewModel model)
    {
        
        await EnsureTokenAsync();
        Console.WriteLine("Intentando crear usuario en Keycloak...");

        var payload = new
        {
            username = model.Email,
            email = model.Email,
            firstName = model.Nombre,
            enabled = true,
            credentials = new[]
            {
                new {
                    type = "password",
                    value = model.Password,
                    temporary = false
                }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

        var realm = _config["Keycloak:Realm"];
        Console.WriteLine($"🌐 Realm desde configuración: {realm}");

        var response = await _httpClient.PostAsync($"/admin/realms/{realm}/users", content);

        Console.WriteLine($"🛠️ Respuesta de creación: {(int)response.StatusCode} - {response.ReasonPhrase}");

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine("⚠️ Error al crear usuario en Keycloak:");
            Console.WriteLine(errorBody);
            return null;
        }

        var location = response.Headers.Location?.ToString();
        Console.WriteLine($"✅ Usuario creado. Location: {location}");

        var userId = location?.Split('/').Last();
        return userId!;

    }

    public async Task<bool> AssignRoleAsync(string userId, string roleName)
    {
        await EnsureTokenAsync();
        var realm = _config["Keycloak:Realm"];
        var clientId = await GetClientIdAsync();

        Console.WriteLine($"🛠️ Intentando asignar rol '{roleName}' al usuario {userId}");

        // Intentar como rol de cliente
        var clientRoleResponse = await _httpClient.GetAsync($"/admin/realms/{realm}/clients/{clientId}/roles/{roleName}");

        if (clientRoleResponse.IsSuccessStatusCode)
        {
            var clientRole = JsonDocument.Parse(await clientRoleResponse.Content.ReadAsStringAsync()).RootElement;

            var roleObj = new[]
            {
            new
            {
                id = clientRole.GetProperty("id").GetString(),
                name = clientRole.GetProperty("name").GetString()
            }
        };

            Console.WriteLine($"🎯 Asignando rol '{roleName}' (Cliente ID: {roleObj[0].id}) al usuario con ID: {userId}");

            var content = new StringContent(JsonSerializer.Serialize(roleObj), Encoding.UTF8, "application/json");

            var assignResponse = await _httpClient.PostAsync(
                $"/admin/realms/{realm}/users/{userId}/role-mappings/clients/{clientId}", content
            );

            Console.WriteLine($"🧾 Resultado de asignación (cliente): {(assignResponse.IsSuccessStatusCode ? "✅ Exitoso" : "❌ Falló")}");
            return assignResponse.IsSuccessStatusCode;
        }

        // Si no se encuentra en cliente, intentar como rol de realm
        Console.WriteLine($"🔁 Rol '{roleName}' no encontrado en cliente. Intentando como rol de realm...");

        var realmRoleResponse = await _httpClient.GetAsync($"/admin/realms/{realm}/roles/{roleName}");

        if (realmRoleResponse.IsSuccessStatusCode)
        {
            var realmRole = JsonDocument.Parse(await realmRoleResponse.Content.ReadAsStringAsync()).RootElement;

            var roleObj = new[]
            {
            new
            {
                id = realmRole.GetProperty("id").GetString(),
                name = realmRole.GetProperty("name").GetString()
            }
        };

            Console.WriteLine($"🌐 Asignando realm-role '{roleName}' (ID: {roleObj[0].id}) al usuario {userId}");

            var content = new StringContent(JsonSerializer.Serialize(roleObj), Encoding.UTF8, "application/json");

            var assignResponse = await _httpClient.PostAsync(
                $"/admin/realms/{realm}/users/{userId}/role-mappings/realm", content
            );

            Console.WriteLine($"🧾 Resultado de asignación (realm): {(assignResponse.IsSuccessStatusCode ? "✅ Exitoso" : "❌ Falló")}");
            return assignResponse.IsSuccessStatusCode;
        }

        Console.WriteLine($"❌ No se encontró el rol '{roleName}' ni en cliente ni en realm.");
        return false;
    }


    private async Task EnsureTokenAsync()
    {
        if (!string.IsNullOrEmpty(_accessToken)) return;

        var tokenUrl = _config["Keycloak:Authority"] + "/protocol/openid-connect/token";
        var clientId = _config["Keycloak:AdminClientId"];
        var clientSecret = _config["Keycloak:AdminClientSecret"];
        var realm = _config["Keycloak:Realm"];

        var body = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "grant_type", "client_credentials" }
        });

        var response = await _httpClient.PostAsync(tokenUrl, body);
        var json = await response.Content.ReadAsStringAsync();
        var token = JsonDocument.Parse(json).RootElement;
        _accessToken = token.GetProperty("access_token").GetString();
    }

    private async Task<string> GetClientIdAsync()
    {
        var realm = _config["Keycloak:Realm"];
        var clientName = _config["Keycloak:AdminClientId"];

        var response = await _httpClient.GetAsync($"/admin/realms/{realm}/clients");
        var clients = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

        var client = clients.EnumerateArray().FirstOrDefault(c => c.GetProperty("clientId").GetString() == clientName);
        return client.GetProperty("id").GetString()!;
    }
}
