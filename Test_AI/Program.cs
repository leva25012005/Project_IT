using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Test_AI.Services;
using Test_AI.Utils;
using Microsoft.Extensions.Configuration.Json;

namespace Test_AI
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Thiết lập cấu hình
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            // Thiết lập Dependency Injection
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IConfiguration>(configuration)
                .AddSingleton<GeminiService>()
                .AddSingleton<DatabaseSchemaService>()
                .AddSingleton<PromptBuilder>()
                .AddSingleton<QueryGenerationService>()
                .BuildServiceProvider();

            // Lấy service
            var queryGenerationService = serviceProvider.GetRequiredService<QueryGenerationService>();

            Console.WriteLine("Chào mừng đến với hệ thống tạo câu truy vấn SQL từ ngôn ngữ tự nhiên!");
            Console.WriteLine("Nhập câu hỏi của bạn (hoặc 'exit' để thoát):");

            while (true)
            {
                Console.Write("> ");
                var query = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(query) || query.ToLower() == "exit")
                {
                    break;
                }

                try
                {
                    Console.WriteLine("Đang xử lý...");
                    var sqlQuery = await queryGenerationService.GenerateSqlQueryFromNaturalLanguageAsync(query);

                    Console.WriteLine("\nCâu truy vấn SQL:");
                    Console.WriteLine(sqlQuery);
                    Console.WriteLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi: {ex.Message}");
                }
            }
        }
    }
}
