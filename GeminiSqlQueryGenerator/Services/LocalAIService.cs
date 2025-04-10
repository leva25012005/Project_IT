using GeminiSqlQueryGenerator.Config;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GeminiSqlQueryGenerator.Services
{
    public class LocalAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _localAiEndpoint;
        private readonly string _modelType;
        private readonly string _modelName;
        private readonly IAIModelAdapter _adapter;
        public LocalAIService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _localAiEndpoint = configuration["LocalAI:ApiEndpoint"];
            _modelType = configuration["LocalAI:ModelType"];
            _modelName = configuration["LocalAI:ModelName"];

            _adapter = AIAdapterFactory.GetAdapter(_modelType);

            if (string.IsNullOrEmpty(_localAiEndpoint))
            {
                throw new ArgumentException("Local AI endpoint must be configured");
            }
        }

        public async Task<string> GenerateSqlQueryAsync(string naturalLanguageQuery, string databaseSchemaJson)
        {
            var prompt = $"Tôi có lược đồ cơ sở dữ liệu như sau:\n\n{databaseSchemaJson}\n\nHãy tạo câu truy vấn SQL cho câu hỏi sau: {naturalLanguageQuery}\n\nChỉ trả về câu truy vấn SQL, không kèm theo giải thích.";

            // Thay đổi format request để bao gồm trường messages
            var request = new
            {
                model = _modelName,
                messages = new[]
                {
            new { role = "system", content = "Bạn là một chuyên gia SQL giúp chuyển đổi câu hỏi ngôn ngữ tự nhiên thành câu truy vấn SQL chính xác." },
            new { role = "user", content = prompt }
        },
                temperature = 0.1,
                max_tokens = 1024
            };

            var requestContent = new StringContent(
                JsonConvert.SerializeObject(request),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(_localAiEndpoint, requestContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Local AI request failed: {response.StatusCode}, {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            // Parse response phù hợp với format của API
            var aiResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);

            // Truy cập kết quả - điều chỉnh theo cấu trúc response của API bạn đang sử dụng
            string result = aiResponse.choices?[0]?.message?.content?.ToString() ?? string.Empty;

            return result.Trim();
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                // Tạo một request đơn giản để kiểm tra kết nối
                var testRequest = new
                {
                    model = _modelName,
                    messages = new[]
                    {
                new { role = "user", content = "Hello" }
            }
                };

                var requestContent = new StringContent(
                    JsonConvert.SerializeObject(testRequest),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(_localAiEndpoint, requestContent);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Không thể kết nối đến API: {ex.Message}");
                return false;
            }
        }
    }
}
