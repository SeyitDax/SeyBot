using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

public class OpenAIHelper
{
    private readonly string _apiKey;
    private readonly string _model;
    private readonly HttpClient _httpClient;

    public OpenAIHelper(IConfiguration configuration)
    {
        var openAiConfig = configuration.GetSection("OpenAI");
        _apiKey = openAiConfig["ApiKey"];
        _model = openAiConfig["Model"] ?? "gpt-3.5-turbo";
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<string> GetChatCompletionAsync(string prompt)
    {
        var requestBody = new
        {
            model = _model,
            messages = new[] { new { role = "user", content = prompt } }
        };
        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseString);
        return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
    }
}