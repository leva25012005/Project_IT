using GeminiSqlQueryGenerator.Models;
using System.Threading.Tasks;
using GeminiSqlQueryGenerator.Utils;
using System;

namespace GeminiSqlQueryGenerator.Services
{
    public class QueryGenerationService
    {
        // Cho API
        /*
        private readonly GeminiService _geminiService;
        private readonly DatabaseSchemaService _databaseSchemaService;
        private readonly PromptBuilder _promptBuilder;

        public QueryGenerationService(
            GeminiService geminiService,
            DatabaseSchemaService databaseSchemaService,
            PromptBuilder promptBuilder)
        {
            _geminiService = geminiService;
            _databaseSchemaService = databaseSchemaService;
            _promptBuilder = promptBuilder;
        }

        public async Task<string> GenerateSqlQueryFromNaturalLanguageAsync(string naturalLanguageQuery)
        {
            // Lấy schema từ database
            var databaseSchema = await _databaseSchemaService.GetDatabaseSchemaAsync();

            // Chuyển schema thành JSON
            var schemaJson = _databaseSchemaService.GetDatabaseSchemaAsJson(databaseSchema);

            // Tối ưu và định dạng prompt
            var optimizedSchema = _promptBuilder.OptimizeSchemaForPrompt(schemaJson);

            // Gọi API Gemini để tạo câu truy vấn SQL
            var sqlQuery = await _geminiService.GenerateSqlQueryAsync(naturalLanguageQuery, optimizedSchema);

            return sqlQuery;
        }*/

        // Cho Local AI
        private readonly LocalAIService _localAIService;
        private readonly DatabaseSchemaService _databaseSchemaService;
        private readonly PromptBuilder _promptBuilder;

        public QueryGenerationService(
            LocalAIService localAIService,
            DatabaseSchemaService databaseSchemaService,
            PromptBuilder promptBuilder)
        {
            _localAIService = localAIService;
            _databaseSchemaService = databaseSchemaService;
            _promptBuilder = promptBuilder;
        }

        public async Task<string> GenerateSqlQueryFromNaturalLanguageAsync(string naturalLanguageQuery)
        {
            try
            {
                // Lấy schema từ database
                 var databaseSchema = await _databaseSchemaService.GetDatabaseSchemaAsync();

                // Chuyển schema thành JSON
                var schemaJson = _databaseSchemaService.GetDatabaseSchemaAsJson(databaseSchema);

                // Tối ưu và định dạng prompt
                var optimizedSchema = _promptBuilder.OptimizeSchemaForPrompt(schemaJson);

                // Gọi Local AI để tạo câu truy vấn SQL
                var sqlQuery = await _localAIService.GenerateSqlQueryAsync(naturalLanguageQuery, optimizedSchema);

                return sqlQuery;
            }
            catch (Exception ex)
            {
                // Log và xử lý lỗi
                Console.WriteLine($"Lỗi khi tạo câu truy vấn SQL: {ex.Message}");

                // Đặt điểm breakpoint ở đây để debug khi chạy
                // Có thể trả về thông báo lỗi hoặc throw lại exception tùy theo thiết kế của bạn
                throw;
            }
        }
    }
}
