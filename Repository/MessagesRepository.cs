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
            IEnumerable<Messages> messages = await _context.Messages
                .Where(m => m.CreatedAt >= message.CreatedAt && message.ConversationId == m.ConversationId)
                .ToListAsync();

            _context.Messages.RemoveRange(messages);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Messages>> GetMessagesByConversationIdAsync(int conversationId)
        {
            return await _context.Messages
                .Where(m => m.ConversationId == conversationId)
                .ToListAsync();
        }
    }
}
