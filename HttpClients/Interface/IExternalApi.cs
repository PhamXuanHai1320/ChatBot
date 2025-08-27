namespace Chat.HttpClients.Interface
{
    public interface IExternalApi
    {
        Task<string> GetContextAsync(string query, string Model);
    }
}
