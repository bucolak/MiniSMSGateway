using MassTransit;
using Microsoft.Extensions.Logging;

namespace Sms;

public class SmsConsumer: IConsumer<SendSmsMassitTransit>
{
    private readonly ILogger<SmsConsumer> _logger;
    private readonly MyDbContext _dbContext;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public SmsConsumer(ILogger<SmsConsumer> logger, MyDbContext dbContext, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _logger = logger;
        _dbContext = dbContext;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<SendSmsMassitTransit> context)
    {
        var messageId = context.Message.MessageId;
        var sms = await _dbContext.Messages.FindAsync(messageId);

        if (sms is null)
            return;

        var request = new BaseSmsBulkRequest
        {
            Credential = new SmsCredential
            {
                Password = _configuration.GetSection("SmsProvider")["Password"],
                Username = _configuration.GetSection("SmsProvider")["Username"]
            },
            Header = new SmsHeader { },
            Envelopes = [new SmsEnvelopes { To = sms.PhoneNumber, Message = sms.Content }]
        };
        try
        {
            var (isSuccess, responseBody) = await Provider.Maradit.SendBulkRequestAsync(_httpClientFactory, request);

            if (isSuccess)
            {
                sms.Status = Enum.MessageStatus.Sent;
                sms.SentAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("[MassTransit Consumer] SMS Başarıyla Gönderildi! ID: {Id}", messageId);
            }
            else
            {
                sms.Status = Enum.MessageStatus.Failed;
                await _dbContext.SaveChangesAsync();
                throw new Exception($"SMS trasmission failed : {responseBody}");
            }
        }
        catch (Exception e)
        {
            sms.Status = Enum.MessageStatus.Failed;
            await _dbContext.SaveChangesAsync();
            _logger.LogError(e, "[MassTransit Consumer] Hata oluştu (ID: {Id})", messageId);
            throw; // MassTransit'in Retry ve Dead-Letter çalıştırması için zorunlu:
        }
    }
}
