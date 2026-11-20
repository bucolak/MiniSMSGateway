using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;

namespace Sms
{
    public class Helper
    {
        public static async Task<int> ReplayFailedMessagesAsync(IConnection connection, IConfiguration configuration, MyDbContext dbContext)
        {
            using var channel = await connection.CreateChannelAsync();

            int count = 0;
            var failedQueue = configuration.GetSection("RabbitMQ")["FailedQueueName"];
            var mainQueue = configuration.GetSection("RabbitMQ")["QueueName"];
            var properties = new BasicProperties
            {
                DeliveryMode = DeliveryModes.Persistent
            };

            while (true)
            {
                var result = await channel.BasicGetAsync(failedQueue, autoAck: false);

                if (result is null)
                    break;

                var body = result.Body.ToArray();
                var getMessageId = Encoding.UTF8.GetString(body);
                var messageId = int.Parse(getMessageId);
                var message = await dbContext.Messages.FindAsync(messageId);
                if (message is not null)
                {
                    message.RetryCount = 0;
                    message.Status = Sms.Enum.MessageStatus.Processing;
                    await dbContext.SaveChangesAsync();
                }
                try
                {
                    await channel.BasicPublishAsync(
                        exchange: string.Empty,
                        routingKey: mainQueue,
                        mandatory: false,
                        basicProperties: properties,
                        body: body
                    );
                    await channel.BasicAckAsync(result.DeliveryTag, multiple: false);
                    count++;
                }
                catch (Exception e)
                {
                    await channel.BasicNackAsync(result.DeliveryTag, multiple: false, true);
                    break;
                }
            }
            return count;
        }

        public static async Task<int> ReplayFailedMassTransitMessagesAsync(IConnection connection, IConfiguration configuration, MyDbContext dbContext)
        {
            using var channel = await connection.CreateChannelAsync();

            int count = 0;
            var mainQueue = configuration.GetSection("MassTransitRabbitMQ")["QueueName"];
            var failedQueue = $"{mainQueue}_error"; // MassTransit varsayılan hata kuyruğu
            var properties = new BasicProperties
            {
                DeliveryMode = DeliveryModes.Persistent
            };

            while(true)
            {
                var result = await channel.BasicGetAsync(failedQueue, autoAck: false);

                if (result is null)
                    break;

                using var doc = JsonDocument.Parse(result.Body);
                if(doc.RootElement.TryGetProperty("message", out var message) &&
                    message.TryGetProperty("messageId", out var id))
                {
                    var messageId = id.GetInt32();
                    var sms = await dbContext.Messages.FindAsync(messageId);
                    if(sms is not null)
                    {
                        sms.Status = Enum.MessageStatus.Pending;
                        await dbContext.SaveChangesAsync();
                    }
                }

                try
                {
                    await channel.BasicPublishAsync(
                        exchange: string.Empty,
                        routingKey: mainQueue,
                        mandatory: false,
                        basicProperties: properties,
                        body: result.Body
                    );
                    await channel.BasicAckAsync(result.DeliveryTag, multiple: false);
                    count++;
                }
                catch (Exception e)
                {
                    await channel.BasicNackAsync(result.DeliveryTag, multiple: false, true);
                    break;
                }
            }
                return count;
        }
    }
}
