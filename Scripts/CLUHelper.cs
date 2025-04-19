using System;
using System.Text;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;

public class CLUHelper
{
    private readonly string _endpoint;
    private readonly string _apiKey;
    private readonly string _projectName;
    private readonly string _deploymentName;
    private readonly HttpClient _httpClient;

    public CLUHelper(IConfiguration configuration)
    {
        var cluConfig = configuration.GetSection("CLU");

        _endpoint = cluConfig["Endpoint"];
        _apiKey = cluConfig["ApiKey"];
        _projectName = cluConfig["ProjectName"];
        _deploymentName = cluConfig["DeploymentName"];
        _httpClient = new HttpClient();
    }

    public async Task<Dictionary<string, string>> ExtractUserDetailsAsync(string userInput)
    {
        var userDetails = new Dictionary<string, string>();

        var jsonRequest = JsonConvert.SerializeObject(new
        {
            kind = "Conversation",
            analysisInput = new
            {
                conversationItem = new
                {
                    text = userInput,
                    id = "1",
                    participantId = "User"
                }
            },
            parameters = new
            {
                projectName = _projectName,
                deploymentName = _deploymentName
            }
        });

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, _endpoint)
        {
            Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json")
        };

        requestMessage.Headers.Add("Ocp-Apim-Subscription-Key", _apiKey);
        var response = await _httpClient.SendAsync(requestMessage);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseBody);

            var topIntent = jsonResponse?.result?.prediction?.topIntent;
            userDetails["Intent"] = topIntent;

            var entities = jsonResponse?.result?.prediction?.entities;

            foreach (var entity in entities)
            {
                string category = entity.category;
                string value = entity.text;

                if (!userDetails.ContainsKey(category))
                {
                    userDetails[category] = value;
                }
            }
        }

        return userDetails;
    }
}

