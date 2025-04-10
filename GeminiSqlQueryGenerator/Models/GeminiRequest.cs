using System.Collections.Generic;

namespace GeminiSqlQueryGenerator.Models
{
    public class GeminiRequest
    {
        public List<Content> Contents { get; set; }
        public GenerationConfig GenerationConfig { get; set; }
    }

    public class Content
    {
        public string Role { get; set; }
        public List<Part> Parts { get; set; }
    }

    public class Part
    {
        public string Text { get; set; }
    }

    public class GenerationConfig
    {
        public double Temperature { get; set; }
        public int MaxOutputTokens { get; set; }
    }
}
