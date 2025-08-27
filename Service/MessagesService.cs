using AutoMapper;
using Chat.DTOS;
using Chat.HttpClients.Interface;
using Chat.Models;
using Chat.Repository.Interfaces;
using Chat.Service.Interfaces;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BotChat.Service
{
    public class MessagesService : IMessagesService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IExternalApi _externalApi;
        public MessagesService(IUnitOfWork unitOfWork, IMapper mapper, IExternalApi externalApi)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _externalApi = externalApi;
        }
        public async Task<String> AddMessageAsync(MessagesDTO messagesDTO)
        {
            try
            {
                messagesDTO.User = "User";
                var result = await AddAsync(messagesDTO);

                string Model = SelectModel(messagesDTO.Content);

                string content = await _externalApi.GetContextAsync(messagesDTO, Model);

                var newMessageDTO = new MessagesDTO
                {
                    User = "AI",
                    Content = content,
                    ConversationId = messagesDTO.ConversationId
                };
                var newMessageResult = await AddAsync(newMessageDTO);
                return content;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task DeleteMessageAsync(int id)
        {
            try
            {
                var message = await _unitOfWork.MessagesRepository.GetById(id);
                if (message == null)
                {
                    throw new KeyNotFoundException($"Message with ID {id} not found.");
                }
                await _unitOfWork.MessagesRepository.DeleteAsync(message);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception("An error occurred while deleting the message.", ex);
            }
        }
        public async Task<IEnumerable<MessagesDTO>> GetMessagesByConversationIdAsync(int conversationId)
        {
            try
            {
                var messages = await _unitOfWork.MessagesRepository.GetMessagesByConversationIdAsync(conversationId);
                if (messages == null)
                {
                    throw new KeyNotFoundException("No messages found.");
                }
                return _mapper.Map<IEnumerable<MessagesDTO>>(messages);
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception("An error occurred while retrieving all messages.", ex);
            }
        }
        public async Task<MessagesDTO> GetMessageByIdAsync(int id)
        {
            try
            {
                var message = await _unitOfWork.MessagesRepository.GetById(id);
                if (message == null)
                {
                    throw new KeyNotFoundException($"Message with ID {id} not found.");
                }
                return _mapper.Map<MessagesDTO>(message);
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception("An error occurred while retrieving the message by ID.", ex);
            }
        }

        public async Task<String> UpdateMessagesAsync(MessagesDTO messagesDTO)
        {
            try
            {
                var message = await _unitOfWork.MessagesRepository.GetById(messagesDTO.Id);
                messagesDTO.Id = 0;
                if (message == null)
                {
                    throw new KeyNotFoundException($"Message with ID {messagesDTO.Id} not found.");
                }

                await _unitOfWork.MessagesRepository.EditMessageAsync(message);

                string content = await AddMessageAsync(messagesDTO);
                return content;
            } catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        private async Task<MessagesDTO> AddAsync(MessagesDTO messagesDTO)
        {
            try
            {
                var message = _mapper.Map<Messages>(messagesDTO);
                if (message == null)
                {
                    throw new ArgumentNullException(nameof(messagesDTO), "Message data cannot be null.");
                }
                await _unitOfWork.MessagesRepository.AddAsync(message);
                await _unitOfWork.SaveChangesAsync();
                return messagesDTO;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception("An error occurred while adding the message.", ex);
            }
        }
        private String SelectModel(string query)
        {
            string[] keyWords = { "tính toán", "giải thích", "phân tích", "toán học", "chứng minh", "lập luận" };
            foreach (var keyword in keyWords)
            {
                if (query.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    return "DeepSeek-R1";
                }
            }
            return "DeepSeek-V3";
        }
    }
}
