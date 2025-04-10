using System.Collections.Generic;

namespace GeminiSqlQueryGenerator.Models
{
    public class GeminiResponse
    {
        public List<Candidate> Candidates { get; set; }
    }

    public class Candidate
    {
        public Content Content { get; set; }
    }
}
