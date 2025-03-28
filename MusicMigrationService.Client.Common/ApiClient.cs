using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MusicMigrationService.WebHost.Models;
using Newtonsoft.Json;

namespace MusicMigrationService.Client.Common;

public static class ApiClient
{
    public static string? BaseUrl { get; set; }
    public static int Port { get; set; }
    
    private static HttpClient? _client;

    public static void Initialize(string baseUrl)
    {
        _client = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
        };
     _client.DefaultRequestHeaders.Accept.Clear();
     _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public static async Task<OneOf<MigrationStatus, string>> GetMigrationStatusAsync(string jobId)
    {
        var response = await _client!.GetAsync($"{_client.BaseAddress}/migration/status/{jobId}");
        response.EnsureSuccessStatusCode();
        
        string jsonContent = await response.Content.ReadAsStringAsync();
        var status = JsonConvert.DeserializeObject<MigrationStatus>(jsonContent);
        
        if (status == null)
            return new OneOf<MigrationStatus, string>.Second("response is null");
        
        return new OneOf<MigrationStatus, string>.First(status);
    }
    
    public static async Task<HttpStatusCode> PostMigrationAsync(MigrationRequest request)
    {
        var response = await _client!.PostAsJsonAsync($"{_client!.BaseAddress}/start", request);
        response.EnsureSuccessStatusCode();
        
        return response.StatusCode;
    }
}