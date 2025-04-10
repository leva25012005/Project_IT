using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Test_AI.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Test_AI.Services
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _apiEndpoint;

        public GeminiService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _apiKey = configuration["Gemini:ApiKey"];
            _apiEndpoint = configuration["Gemini:ApiEndpoint"];

            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_apiEndpoint))
            {
                throw new ArgumentException("Gemini API key and endpoint must be configured in appsettings.json");
            }
        }

        public async Task<string> GenerateSqlQueryAsync(string naturalLanguageQuery, string databaseSchemaJson)
        {
            var prompt = $"Tôi có lược đồ cơ sở dữ liệu như sau:\n\n{databaseSchemaJson}\n\nHãy tạo câu truy vấn SQL cho câu hỏi sau: {naturalLanguageQuery}\n\nChỉ trả về câu truy vấn SQL, không kèm theo giải thích.";

            var request = new GeminiRequest
            {
                Contents = new List<Content>
                {
                    new Content
                    {
                        Role = "user",
                        Parts = new List<Part>
                        {
                            new Part { Text = prompt }
                        }
                    }
                },
                GenerationConfig = new GenerationConfig
                {
                    Temperature = 0.1,
                    MaxOutputTokens = 1024
                }
            };

            var requestContent = new StringContent(
                JsonConvert.SerializeObject(request),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(
                $"{_apiEndpoint}?key={_apiKey}",
                requestContent
            );

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Gemini API request failed: {response.StatusCode}, {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var geminiResponse = JsonConvert.DeserializeObject<GeminiResponse>(responseContent);

            if (geminiResponse?.Candidates == null || geminiResponse.Candidates.Count == 0)
            {
                throw new Exception("No response received from Gemini API");
            }

            return geminiResponse.Candidates[0].Content.Parts[0].Text.Trim();
        }
    }
}
