using Chat.Models;
using Chat.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Chat.Repository
{
    public class MessagesRepository : Repository<Messages>, IMessagesRepository
    {
        public MessagesRepository(Data.ApplicationDbContext context) : base(context)
        {
        }

        public async Task EditMessageAsync(Messages message)
        {
            await _context.Messages
                .Where(m => m.CreatedAt >= message.CreatedAt && message.ConversationId == m.ConversationId)
                .ExecuteDeleteAsync();
        }

        public async Task<IEnumerable<Messages>> GetMessagesByConversationIdAsync(int conversationId)
        {
            return await _context.Messages
                .FromSqlRaw("SELECT * FROM Messages WHERE ConversationId = {0} ORDER BY CreatedAt ASC", conversationId)
                .ToListAsync();
        }
    }
}
