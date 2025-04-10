using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeminiSqlQueryGenerator.Config
{
    public interface IAIModelAdapter
    {
        string ExtractResponse(string jsonResponse);
    }

    public class OllamaAdapter : IAIModelAdapter
    {
        public string ExtractResponse(string jsonResponse)
        {
            var response = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
            return response.response.ToString();
        }
    }

    public class LlamaAdapter : IAIModelAdapter
    {
        public string ExtractResponse(string jsonResponse)
        {
            var response = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
            return response.generations[0].text.ToString();
        }
    }
    public class DefaultAdapter : IAIModelAdapter
    {
        public string ExtractResponse(string jsonResponse)
        {
            return string.Empty;
        }
    }

    public class AIAdapterFactory
    {
        public static IAIModelAdapter GetAdapter(string modelType)
        {
            switch (modelType.ToLower())
            {
                case "ollama":
                    return new OllamaAdapter();
                case "llama":
                    return new LlamaAdapter();
                default:
                    return new DefaultAdapter();
            }
        }
    }

}
