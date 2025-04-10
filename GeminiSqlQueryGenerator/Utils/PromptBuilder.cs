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
            sb.AppendLine("## Dưới đây là lược đồ cơ sở dữ liệu:");
            sb.AppendLine(schemaDescription);
            sb.AppendLine();

            sb.AppendLine("## Hãy tạo câu truy vấn SQL cho câu hỏi sau:");
            sb.AppendLine(userQuery);
            sb.AppendLine();

            sb.AppendLine("## Hướng dẫn:");
            sb.AppendLine("1. Phân tích kỹ lược đồ CSDL trước khi viết truy vấn.");
            sb.AppendLine("2. Nếu có cột hoặc bảng được nhắc đến trong câu hỏi không tồn tại trong CSDL, hãy trả về thông báo lỗi rõ ràng với định dạng: ERROR: [chi tiết lỗi]");
            sb.AppendLine("3. Đối với truy vấn phức tạp, hãy sử dụng Common Table Expressions (CTE) để làm rõ các bước trung gian.");
            sb.AppendLine("4. Sử dụng JOIN khi cần kết nối nhiều bảng, chỉ rõ loại JOIN (INNER, LEFT, RIGHT).");
            sb.AppendLine("5. Sử dụng các hàm tổng hợp (SUM, COUNT, AVG) khi câu hỏi yêu cầu tính toán số liệu.");
            sb.AppendLine("6. Tối ưu câu truy vấn để tránh subquery không cần thiết.");
            sb.AppendLine("7. Sử dụng chính xác các từ khóa như WHERE, GROUP BY, HAVING, ORDER BY khi cần thiết.");
            sb.AppendLine("8. Xác định và xử lý rõ các điều kiện so sánh (=, <>, >, <, LIKE, etc.).");
            sb.AppendLine("9. Đặt tên alias cho các bảng và cột một cách rõ ràng.");
            sb.AppendLine();

            sb.AppendLine("## Phản hồi:");
            sb.AppendLine("1. Nếu các điều kiện đều hợp lệ, chỉ trả về câu truy vấn SQL cuối cùng (không kèm phân tích).");
            sb.AppendLine("2. Nếu phát hiện lỗi, trả về thông báo lỗi với định dạng 'ERROR: [chi tiết lỗi]'.");

            return sb.ToString();
        }
    }
}
