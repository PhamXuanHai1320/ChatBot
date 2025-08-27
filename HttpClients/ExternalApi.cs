using Chat.DTOS;
using Chat.HttpClients.Interface;
using Chat.Models;
using Chat.Repository.Interfaces;
using System.Text;
using System.Text.Json;

namespace Chat.HttpClients
{
    public class ExternalApi : IExternalApi
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        public ExternalApi(IHttpClientFactory httpClientFactory, IConfiguration configuration, IUnitOfWork unitOfWork)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
            _unitOfWork = unitOfWork;
        }

        public async Task<string> GetContextAsync(MessagesDTO messageDTO, string Model)
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
                messages = await Content(messageDTO)
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

        private async Task<List<object>> Content(MessagesDTO messageDTO)
        {
            IEnumerable<Messages> messages = await _unitOfWork.MessagesRepository
                .GetMessagesByConversationIdAsync(messageDTO.ConversationId);
            var apiMessages = new List<object>();

            foreach (var msg in messages)
            {
                apiMessages.Add(new
                {
                    role = "assistant",
                    content = msg.Content
                });
            }

            apiMessages.Add(
                new { role = "system", content = "Bạn là một ChatBot thân thiện và chuyên nghiệp. " +
                    "Luôn bắt đầu câu trả lời bằng lời chào hoặc lời cảm ơn. " +
                    "Trả lời chỉ dùng văn bản thuần (plain text), tuyệt đối không được dùng bất kỳ ký hiệu markdown nào như *, #, **, ###. " +
                    "Khi gặp phép tính toán thì phải trình bày rõ ràng, chi tiết và có giải thích. " +
                    "Bạn luôn ưu tiên thông tin chính xác, mới nhất tính đến năm 2025."}
            );
            apiMessages.Add(
                new { role = "user", content = messageDTO.Content }
            );
            return apiMessages;
        }
    }
}