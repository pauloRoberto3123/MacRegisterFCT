using MacRegister.Dtos;
using MacRegister.InfraStructure;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;



public class JabilApi
{
    private readonly HttpClient _httpClient;

    public JabilApi(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(Constants.BaseUrlMes);
    }

    public async Task<string> SendTestMes(MesApiRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("JabilAPI/Test/SendTestMes", content);

            response.EnsureSuccessStatusCode(); // Throws on error response.

            return await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Failed to send Test MES request. {ex.Message}", ex);
        }
    }
}
