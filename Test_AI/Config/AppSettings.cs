using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_AI.Config
{
    public class GeminiSettings
    {
        public string ApiKey { get; set; }
        public string ApiEndpoint { get; set; }
    }

    public class AppSettings
    {
        public GeminiSettings Gemini { get; set; }
    }
}
