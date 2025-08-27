using Chat.DTOS;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;

namespace Chat.HttpClients.Interface
{
    public interface IExternalApi
    {
        Task<string> GetContextAsync(MessagesDTO messageDTO, string Model);
    }
}
