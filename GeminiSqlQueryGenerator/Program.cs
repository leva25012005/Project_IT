using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GeminiSqlQueryGenerator.Utils;
using Microsoft.Extensions.Configuration.Json;
using GeminiSqlQueryGenerator.Services;

namespace GeminiSqlQueryGenerator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                // Thiết lập cấu hình
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build();

                // Thiết lập Dependency Injection
                // Cho API
                /*var serviceProvider = new ServiceCollection()
                    .AddSingleton<IConfiguration>(configuration)
                    .AddSingleton<GeminiService>()
                    .AddSingleton<DatabaseSchemaService>()
                    .AddSingleton<PromptBuilder>()
                    .AddSingleton<QueryGenerationService>()
                    .BuildServiceProvider();*/
                // Cho Local AI
                var serviceProvider = new ServiceCollection()
                    .AddSingleton<IConfiguration>(configuration)
                    .AddSingleton<LocalAIService>()
                    .AddSingleton<DatabaseSchemaService>()
                    .AddSingleton<PromptBuilder>()
                    .AddSingleton<QueryGenerationService>()
                    .BuildServiceProvider();

                var localAIService = serviceProvider.GetRequiredService<LocalAIService>();
                var isConnected = await localAIService.TestConnectionAsync();
                if (!isConnected)
                {
                    Console.WriteLine("Không thể kết nối đến API AI. Vui lòng kiểm tra lại endpoint và đảm bảo dịch vụ AI đang chạy.");
                    Console.WriteLine($"Endpoint hiện tại: {configuration["LocalAI:ApiEndpoint"]}");
                    return;
                }

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
                        // Thêm try-catch chi tiết tại điểm gọi hàm
                        try
                        {
                            Console.WriteLine("Đang xử lý...");
                            var sqlQuery = await queryGenerationService.GenerateSqlQueryFromNaturalLanguageAsync(query);
                            if(string.IsNullOrWhiteSpace(sqlQuery))
                            {
                                Console.WriteLine("Không thể tạo câu truy vấn SQL từ câu hỏi này.");
                                continue;
                            }
                            Console.WriteLine("\nCâu truy vấn SQL:");
                            Console.WriteLine(sqlQuery);
                            Console.WriteLine();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Lỗi khi gọi GenerateSqlQueryFromNaturalLanguageAsync: {ex.Message}");
                            Console.WriteLine($"Stack trace: {ex.StackTrace}");

                            if (ex.InnerException != null)
                            {
                                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Lỗi tổng thể: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khởi tạo ứng dụng: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
