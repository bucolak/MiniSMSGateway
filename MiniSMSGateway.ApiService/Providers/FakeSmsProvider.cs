namespace MiniSMSGateway.ApiService.Providers
{
    public class FakeSmsProvider : ISmsProvider
    {
        public async Task<bool> SendAsync(string phoneNumber, string content)
        {
            await Task.Delay(1000);
            return true;
        }
    }
}
