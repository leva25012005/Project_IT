using Test_AI.Models;
using System.Threading.Tasks;
using Test_AI.Utils;

namespace Test_AI.Services
{
    public class QueryGenerationService
    {
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
        }
    }
}
