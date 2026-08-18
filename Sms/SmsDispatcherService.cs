using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Eventing.Reader;

namespace Sms
{
    public class SmsDispatcherService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SmsDispatcherService> _logger;

        public SmsDispatcherService(IServiceScopeFactory scopeFactory, IHttpClientFactory httpClientFactory, ILogger<SmsDispatcherService> logger)
        {
            _scopeFactory = scopeFactory;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingAsync(stoppingToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Sms dispatch loop failed");
                }
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
        }
        private async Task ProcessPendingAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();

            var pending = await dbContext.Messages
                        .Where(s => s.Status == Sms.Enum.MessageStatus.Pending)
                        .OrderBy(i => i.Id)
                        .Take(20)
                        .ToListAsync(ct);

            if (pending.Count < 1)
                return;

            foreach (var p in pending)
                p.Status = Sms.Enum.MessageStatus.Processing;
            await dbContext.SaveChangesAsync(ct);
            var envelopes = pending.Select(p => new SmsEnvelopes
            {
                To = p.PhoneNumber,
                Message = p.Content
            }).ToList();
            var bulkRequests = new BaseSmsBulkRequest
            {
                Credential = new SmsCredential
                {
                    Password = "C8aykU*GX8",
                    Username = "balaban-bulktest"
                },
                Header = new SmsHeader { },
                Envelopes = envelopes
            };
            foreach (var p in pending)
            {
                var (isSuccess, responseBody) = await Helper.SendBulkRequestAsync(_httpClientFactory, bulkRequests);
                try
                {   
                    if (!isSuccess)
                    {
                        p.Status = Sms.Enum.MessageStatus.Failed;
                        _logger.LogWarning("SMS gönderilemedi (id={Id}, numara={Phone}): {Body}", p.Id, p.PhoneNumber, responseBody);
                    }
                    else
                    {
                        p.Status = Sms.Enum.MessageStatus.Sent;
                        p.SentAt = DateTime.UtcNow;
                    }
                }
                catch(Exception e)
                {
                    p.Status = Sms.Enum.MessageStatus.Failed;
                    _logger.LogError(e, "SMS gönderim sırasında hata (id={Id})", p.Id);
                }
            }
            await dbContext.SaveChangesAsync(ct);
        }
    }
}
