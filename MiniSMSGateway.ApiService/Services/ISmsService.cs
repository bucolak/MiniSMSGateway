using MiniSMSGateway.ApiService.DTO;

namespace MiniSMSGateway.ApiService.Services
{
    public interface ISmsService
    {
        Task<SmsStatusResponse?> SendSms(SendSmsRequest request, string apiKey);
        Task<SmsStatusResponse?> GetStatus(int id);
    }
}
