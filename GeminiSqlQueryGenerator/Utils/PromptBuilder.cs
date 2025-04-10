using Newtonsoft.Json;
using System.Text;
using GeminiSqlQueryGenerator.Models;

namespace GeminiSqlQueryGenerator.Utils
{
    public class PromptBuilder
    {
        // Phương thức này tối ưu hóa cách trình bày lược đồ CSDL trong prompt
        public string OptimizeSchemaForPrompt(string schemaJson)
        {
            var schema = JsonConvert.DeserializeObject<DatabaseSchema>(schemaJson);
            var sb = new StringBuilder();

            sb.AppendLine("# Lược đồ cơ sở dữ liệu:");
            sb.AppendLine();

            // Thêm thông tin các bảng và cột
            foreach (var table in schema.Tables)
            {
                sb.AppendLine($"## Bảng: {table.Name}");
                sb.AppendLine("| Tên Cột | Kiểu Dữ Liệu | Khóa Chính | Khóa Ngoại |");
                sb.AppendLine("|---------|--------------|------------|------------|");

                foreach (var column in table.Columns)
                {
                    var isPrimaryKey = column.IsPrimaryKey ? "Có" : "";
                    var isForeignKey = column.IsForeignKey ? "Có" : "";
                    var foreignKeyInfo = column.IsForeignKey ? $" (-> {column.ReferencedTable}.{column.ReferencedColumn})" : "";

                    sb.AppendLine($"| {column.Name} | {column.DataType} | {isPrimaryKey} | {isForeignKey}{foreignKeyInfo} |");
                }

                sb.AppendLine();
            }

            // Thêm thông tin về mối quan hệ
            if (schema.Relationships != null && schema.Relationships.Count > 0)
            {
                sb.AppendLine("## Mối quan hệ:");
                foreach (var rel in schema.Relationships)
                {
                    sb.AppendLine($"- {rel.SourceTable}.{rel.SourceColumn} -> {rel.TargetTable}.{rel.TargetColumn}");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        // Phương thức tạo prompt hướng dẫn Gemini cách tạo câu truy vấn SQL
        public string BuildQueryGenerationPrompt(string schemaDescription, string userQuery)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Bạn là một chuyên gia SQL giúp chuyển đổi câu hỏi bằng ngôn ngữ tự nhiên thành câu truy vấn SQL chính xác.");
            sb.AppendLine();
            sb.AppendLine("Dưới đây là lược đồ cơ sở dữ liệu:");
            sb.AppendLine(schemaDescription);
            sb.AppendLine();
            sb.AppendLine("Hãy tạo câu truy vấn SQL cho câu hỏi sau:");
            sb.AppendLine(userQuery);
            sb.AppendLine();
            sb.AppendLine("Yêu cầu:");
            sb.AppendLine("1. Chỉ trả về câu truy vấn SQL, không kèm theo giải thích.");
            sb.AppendLine("2. Đảm bảo câu truy vấn chính xác về cú pháp và sử dụng đúng cấu trúc của cơ sở dữ liệu.");
            sb.AppendLine("3. Sử dụng JOIN khi cần để kết nối dữ liệu từ nhiều bảng.");
            sb.AppendLine("4. Đặt bí danh (alias) cho các bảng khi cần thiết để tăng độ rõ ràng.");

            return sb.ToString();
        }
    }
}
