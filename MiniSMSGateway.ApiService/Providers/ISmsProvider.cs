namespace MiniSMSGateway.ApiService.Providers
{
    public interface ISmsProvider
    {
        Task<bool> SendAsync(string phoneNumber, string content);
    }
}
