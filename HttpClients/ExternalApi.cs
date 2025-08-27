using Chat.HttpClients.Interface;
using System.Text;
using System.Text.Json;

namespace Chat.HttpClients
{
    public class ExternalApi : IExternalApi
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        public ExternalApi(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
        }

        public async Task<string> GetContextAsync(string query, string Model)
        {
            string url = _configuration["ChatBot:URL"];
            string ApiKey = _configuration["ChatBot:APIKey"];
            // Token
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");
            // Body
            var body = new
            {
                model = Model,
                messages = new[]
                {
                    new { role = "system", content = "Bạn là một ChatBot thân thiện và chuyên nghiệp. Luôn bắt đầu câu trả lời bằng lời chào hoặc lời cảm ơn. Khi trả lời, hãy chỉ dùng văn bản thường, không dùng ký hiệu markdown (*, #, **, ###). Bạn luôn ưu tiên thông tin chính xác, mới nhất tính đến năm 2025."},
                    new { role = "user", content = query}
                }
            };
            string jsonBody = JsonSerializer.Serialize(body);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error fetching data: {responseContent}");
            }

            using var jsonDoc = JsonDocument.Parse(responseContent);
            var root = jsonDoc.RootElement;
            var context = root
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
            return context;
        }
    }
}