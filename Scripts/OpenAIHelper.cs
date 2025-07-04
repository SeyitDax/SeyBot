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
    private readonly IHttpClientFactory _httpClientFactory;

    public OpenAIHelper(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        var openAiConfig = configuration.GetSection("OpenAI");
        _apiKey = openAiConfig["ApiKey"];
        if (string.IsNullOrWhiteSpace(_apiKey))
+        {
+            throw new ArgumentException("OpenAI API key is required but not configured.");
+        }
        _model = openAiConfig["Model"] ?? "gpt-3.5-turbo";
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> GetChatCompletionAsync(string prompt)
    {
        try
        {
            var requestBody = new
            {
                model = _model,
                messages = new[] { new { role = "user", content = prompt } }
            };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                // Optionally log errorContent
                return $"Sorry, I couldn't reach the AI service. (Status: {response.StatusCode})";
            }
            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);
            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        }
        catch (Exception ex)
        {
            // Optionally log ex
            return "Sorry, something went wrong while contacting the AI service.";
        }
    }
}
