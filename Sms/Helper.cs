using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace Sms
{
    public class Helper
    {
        public static async Task<(bool isSuccess, string responseBody)> SendRequestAsync(IHttpClientFactory httpClientFactory, BaseSmsRquest request)
        {
            var client = httpClientFactory.CreateClient("SmsProvider");

            var jsonOptions = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = null
            };
            var jsonBody = System.Text.Json.JsonSerializer.Serialize(request, jsonOptions);

            using var content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");
            var response =  await client.PostAsync("https://sgw.maradit.net/api/json/reply/Submit", content);

            var resp = await response.Content.ReadAsStringAsync();
            return (response.IsSuccessStatusCode,  resp);
        }

        public static async Task<(bool isSuccess, string responseBody)> SendBulkRequestAsync(IHttpClientFactory httpClientFactory, BaseSmsBulkRequest request)
        {
            var client = httpClientFactory.CreateClient("SmsProvider");

            var jsonOptions = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = null
            };
            var jsonBody = System.Text.Json.JsonSerializer.Serialize(request, jsonOptions);

            using var content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://sgw.maradit.net/api/json/reply/SubmitMulti", content);

            var resp = await response.Content.ReadAsStringAsync();
            return (response.IsSuccessStatusCode, resp);
        }

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

            while(true)
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
    }
}
