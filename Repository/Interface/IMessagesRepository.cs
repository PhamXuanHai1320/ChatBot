using Chat.Models;

namespace Chat.Repository.Interfaces
{
    public interface IMessagesRepository : IRepository<Messages>
    {
        Task<IEnumerable<Messages>> GetMessagesByConversationIdAsync(int conversationId);
        Task EditMessageAsync(Messages message);
    }
}
