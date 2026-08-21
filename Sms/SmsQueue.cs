//using RabbitMQ.Client;
//using RabbitMQ.Client.Events;
//using System.Text;
//using System.Threading.Channels;

//namespace Sms
//{
//    public class SmsQueue
//    {
//        private readonly IConnection _connection;
//        private readonly IChannel _channel;
//        public SmsQueue()
//        {
//            var factory = new ConnectionFactory { HostName = "localhost" };
//            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
//            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();


//            _channel.QueueDeclareAsync(
//                queue: "sms_queue",
//                durable: true,
//                exclusive: false,
//                autoDelete: false,
//                arguments: null
//            ).GetAwaiter().GetResult();
//        }

//        public async Task PublishMessageIdAsync(int messageID)
//        {
//            var body = Encoding.UTF8.GetBytes(messageID.ToString());

//            var properties = new BasicProperties
//            {
//                Persistent = true
//            };

//            await _channel.BasicPublishAsync(
//                exchange: string.Empty,
//                routingKey: "sms_queue",
//                mandatory: false,
//                basicProperties: properties,
//                body: body
//            );
//        }
//        public async Task ConsumeMessageIdAsync(Func<int, Task> onMessageReceived)
//        {
//            var consumer = new AsyncEventingBasicConsumer(_channel);
//            consumer.ReceivedAsync += async (model, ea) =>
//            {
//                var body = ea.Body.ToArray();
//                var message = Encoding.UTF8.GetString(body);
//                var messageID = int.Parse(message);

//                await onMessageReceived(messageID);
//                await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
//                await Task.Yield();
//            };

//            await _channel.BasicConsumeAsync(
//                queue: "sms_queue",
//                autoAck: true,
//                consumer: consumer
//            );

//        }
//    }
//}
