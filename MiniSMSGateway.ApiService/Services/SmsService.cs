using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using MiniSMSGateway.ApiService.Data;
using MiniSMSGateway.ApiService.DTO;
using MiniSMSGateway.ApiService.Models;
using MiniSMSGateway.ApiService.Providers;

namespace MiniSMSGateway.ApiService.Services
{
    public class SmsService : ISmsService
    {
        private readonly SmsDbContext _context;
        private readonly ISmsProvider _provider;

        public SmsService(SmsDbContext context, ISmsProvider provider)
        {
            _context = context;
            _provider = provider;
        }
        public async Task<SmsStatusResponse> SendSms(SendSmsRequest request, string apiKey)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.ApiKey == apiKey);
            if (user is null)
                return null;
            var message = new Message
            {
                UserId = user.Id,
                PhoneNumber = request.PhoneNumber,
                Content = request.Content,
                Status = Enums.MessageStatus.Pending,
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            bool status = await _provider.SendAsync(message.PhoneNumber, message.Content);
            Console.WriteLine("Status kontrol: "+status);
            if(status)
            {
                message.Status = Enums.MessageStatus.Sent;
                message.SentAt = DateTime.UtcNow;
            }
            else
            {
                message.Status = Enums.MessageStatus.Failed;
            }

            await _context.SaveChangesAsync();

            var response = new SmsStatusResponse
            {
                Id = message.Id,
                Status = message.Status,
                SentAt = message.SentAt
            };

            return response;
        }

        public async Task<SmsStatusResponse?> GetStatus(int id)
        {
            var message = await _context.Messages.FirstOrDefaultAsync(i => i.Id == id);
            if(message is null)
            {
                return null;
            }
            var response = new SmsStatusResponse
            {
                Id = message.Id,
                Status = message.Status,
                SentAt = message.SentAt
            };
            return response;
        }

    }
}