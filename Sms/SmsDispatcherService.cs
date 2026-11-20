using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics.Eventing.Reader;
using System.Text;

namespace Sms
{
    public class SmsDispatcherService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SmsDispatcherService> _logger;
        private readonly IConnection _connection;
        private readonly IConfiguration _configuration;

        public SmsDispatcherService(IServiceScopeFactory scopeFactory, IHttpClientFactory httpClientFactory, ILogger<SmsDispatcherService> logger, IConnection connection, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _connection = connection;
            _configuration = configuration;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var channel = await _connection.CreateChannelAsync();

            var consumer = new AsyncEventingBasicConsumer(channel);

            await channel.ExchangeDeclareAsync(
                exchange: _configuration.GetSection("RabbitMQ")["DeadLetterExchange"],
                type: ExchangeType.Fanout
            );

            await channel.QueueDeclareAsync(
                queue: _configuration.GetSection("RabbitMQ")["FailedQueueName"],
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            await channel.QueueBindAsync(
                queue: _configuration.GetSection("RabbitMQ")["FailedQueueName"],
                exchange: _configuration.GetSection("RabbitMQ")["DeadLetterExchange"],
                routingKey: string.Empty
            );

            await channel.ExchangeDeclareAsync(
                exchange: _configuration.GetSection("RabbitMQ")["RetryExchange"],
                type: ExchangeType.Fanout
            );

            var retryArguments = new Dictionary<string, object?>
            {
                { "x-dead-letter-exchange", _configuration.GetSection("RabbitMQ")["RetryExchange"] },
                { "x-message-ttl", 15000}
            };

            await channel.QueueDeclareAsync(
                queue: _configuration.GetSection("RabbitMQ")["RetryQueueName"],
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: retryArguments
            );

            var mainArguments = new Dictionary<string, object?>
            {
                { "x-dead-letter-exchange", _configuration.GetSection("RabbitMQ")["DeadLetterExchange"] }
            };

            await channel.QueueDeclareAsync(
                queue: _configuration.GetSection("RabbitMQ")["QueueName"],
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: mainArguments
            );

            await channel.QueueBindAsync(
                queue: _configuration.GetSection("RabbitMQ")["QueueName"],
                exchange: _configuration.GetSection("RabbitMQ")["RetryExchange"],
                routingKey: string.Empty
            );

            consumer.ReceivedAsync += async (model, ea) =>
            {
                await Task.Delay(5000, stoppingToken);
                bool willre_add = false;
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var messageID = int.Parse(message);

                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
                    var sms = await dbContext.Messages.FindAsync(messageID);

                    if (sms is null)
                    {
                        await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                        return;
                    }

                    _logger.LogInformation(
                        "[{Time}] >>> Mesaj İşleniyor (ID: {Id}, Mevcut Retry: {Count})",
                        DateTime.Now.ToString("HH:mm:ss.fff"),
                        messageID,
                        sms?.RetryCount ?? 0
                    );

                    var bulkRequests = new BaseSmsBulkRequest
                    {
                        Credential = new SmsCredential
                        {
                            Password = _configuration.GetSection("SmsProvider")["Password"],
                            Username = _configuration.GetSection("SmsProvider")["Username"]
                        },
                        Header = new SmsHeader { },
                        Envelopes = [new SmsEnvelopes { To = sms.PhoneNumber, Message = sms.Content } ]
                    };
                    try
                    {
                        if(sms.ProviderType == Enum.ProviderType.Maradit)
                        {

                            var (isSuccess, responseBody) = await Provider.Maradit.SendBulkRequestAsync(_httpClientFactory, bulkRequests);
                            if (!isSuccess)
                            {
                                sms.Status = Sms.Enum.MessageStatus.Failed;
                                _logger.LogWarning("SMS gönderilemedi (id={Id}, numara={Phone}): {Body}", sms.Id, sms.PhoneNumber, responseBody);

                            }
                            else
                            {
                                sms.Status = Sms.Enum.MessageStatus.Sent;
                                sms.SentAt = DateTime.UtcNow;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        sms.Status = Sms.Enum.MessageStatus.Failed;
                        _logger.LogError(e, "SMS gönderim sırasında hata (id={Id})", sms.Id);
                        willre_add = true;
                        sms.RetryCount++;
                    }

                    await dbContext.SaveChangesAsync(stoppingToken);

                    if (willre_add)
                    {
                        if (sms.RetryCount >= 3)
                        {
                            sms.Status = Sms.Enum.MessageStatus.Failed;
                            await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, false);
                        }
                        else
                        {
                            var properties = new BasicProperties
                            {
                                DeliveryMode = DeliveryModes.Persistent
                            };
                            await channel.BasicPublishAsync(
                                exchange: string.Empty,
                                routingKey: _configuration.GetSection("RabbitMQ")["RetryQueueName"],
                                body: ea.Body,
                                mandatory: false,
                                basicProperties: properties
                            );
                            await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                        }
                    }
                    else
                        await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                    
                }
            };

            await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

            await channel.BasicConsumeAsync(
                queue: _configuration.GetSection("RabbitMQ")["QueueName"],
                autoAck: false,
                consumer: consumer
            );
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
