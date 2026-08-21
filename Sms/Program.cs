//Console.WriteLine("Hello, World!");

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.VisualBasic;
using Polly;
using RabbitMQ.Client;
using Sms;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;
using System.Xml.Linq;

var builder = WebApplication.CreateBuilder();

/*var connectionstring = builder.Configuration.GetConnectionString("my-db");

builder.Services.AddDbContext<MyDbContext>(options => options.UseNpgsql(connectionstring));*/

builder.AddNpgsqlDbContext<MyDbContext>("my-db");

builder.Services.AddControllers();

builder.Services.AddHttpClient("SmsProvider").AddResilienceHandler("sms-circuit-breaker", builder =>
{
    builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
    {
        SamplingDuration = TimeSpan.FromSeconds(30),
        MinimumThroughput = 3,
        FailureRatio = 0.5,
        BreakDuration = TimeSpan.FromSeconds(15),
    });
});

builder.Services.AddHostedService<SmsDispatcherService>();

var factory = new ConnectionFactory 
{ 
    HostName = "localhost", 
    //AutomaticRecoveryEnabled = true,
    //TopologyRecoveryEnabled = true
};
await using var connection = await factory.CreateConnectionAsync();

builder.Services.AddSingleton(connection);

//builder.Services.AddSingleton<SmsQueue>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();

    dbContext.Database.Migrate();
}

app.MapPost("/api/user", (MyDbContext dbContext, [FromBody] CreateUserRequest request) =>
{
    var user = new User
    {
        Name = request.name,
        ApiKey = Guid.NewGuid().ToString()
    };
    dbContext.Users.Add(user);
    dbContext.SaveChanges();
    return Results.Created($"api/send/{user.Id}", user);
});

app.MapGet("/api/users", (MyDbContext dbContext) =>
    {
        return Results.Ok(dbContext.Users.ToList());
    });

app.MapGet("/api/sms/messages", async (MyDbContext dbContext, [FromHeader(Name = "Api-Key")] string? apiKey) =>
{
    var user = await dbContext.Users.FirstOrDefaultAsync(u => u.ApiKey == apiKey);
    if (user is null)
    {
        return Results.Unauthorized();
    }
    return Results.Ok(dbContext.Messages.Include(m => m.User).ToList());
});

app.MapGet("/api/sms/status/{id}", async (MyDbContext dbContext,int id) =>
{
    var message = await dbContext.Messages.Include(m => m.User).FirstOrDefaultAsync(m => m.Id == id);
    if (message is null)
        return Results.NotFound($"SMS record with ID {id} not found!");
    return Results.Ok(message);
});

app.MapPost("api/sms/send", async (IHttpClientFactory httpClientFactory, MyDbContext dbContext, IConnection connection, [FromHeader(Name="Api-Key")] string? apiKey, [FromBody]SendSmsRequest request) =>
{
    var user = await dbContext.Users.FirstOrDefaultAsync(u => u.ApiKey == apiKey);
    if(user is null)
        return Results.Unauthorized();

    var messageFields = request.to.Select( n => new Message
    {
        UserId = user.Id,
        User = user,
        PhoneNumber = n,
        Content = request.message,
        Status = Sms.Enum.MessageStatus.Pending
    }).ToList();

    dbContext.Messages.AddRange(messageFields);
    await dbContext.SaveChangesAsync();

    //var factory = new ConnectionFactory { HostName = "localhost" };
    //await using var connection = await factory.CreateConnectionAsync();
    await using var channel = await connection.CreateChannelAsync();

    var arguments = new Dictionary<string, object?>
    {
        { "x-dead-letter-exchange", builder.Configuration.GetSection("RabbitMQ")["DeadLetterExchange"] }
    };

    await channel.QueueDeclareAsync(
        queue: builder.Configuration.GetSection("RabbitMQ")["QueueName"],
        durable: true,
        exclusive: false,
        autoDelete: false,
        arguments: arguments
    );

    var properties = new BasicProperties
    {
        DeliveryMode = DeliveryModes.Persistent
    };

    foreach (var m in messageFields)
    {
        var body = Encoding.UTF8.GetBytes(m.Id.ToString());
        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: builder.Configuration.GetSection("RabbitMQ")["QueueName"],
            mandatory: false,
            basicProperties: properties,
            body: body
        );
    }

    var mId = messageFields.Select(m => m.Id);
    return Results.Accepted(value: mId);
});

app.MapPost("api/sms/send-bulk", async (IHttpClientFactory httpClient, MyDbContext dbContext, IConnection connection, [FromHeader(Name = "Api-Key")] string? apiKey, [FromBody] SendSmsBulkRequest request) =>
{
    var user = await dbContext.Users.FirstOrDefaultAsync(u => u.ApiKey == apiKey);
    if (user is null)
        Results.Unauthorized();

    if (request.Envelopes is null || request.Envelopes.Count == 0)
        return Results.BadRequest("There is no Sms!");

    var messageFields = request.Envelopes.Select(env => new Message
    {
        UserId = user.Id,
        User = user,
        PhoneNumber = env.To,
        Content = env.Message,
        Status = Sms.Enum.MessageStatus.Pending
    }).ToList();

    dbContext.Messages.AddRange(messageFields);
    await dbContext.SaveChangesAsync();

    //var factory = new ConnectionFactory { HostName = "localhost" };
    //await using var connection = await factory.CreateConnectionAsync();
    await using var channel = await connection.CreateChannelAsync();

    var arguments = new Dictionary<string, object?>
    {
        { "x-dead-letter-exchange", builder.Configuration.GetSection("RabbitMQ")["DeadLetterExchange"] }
    };

    await channel.QueueDeclareAsync(
        queue: builder.Configuration.GetSection("RabbitMQ")["QueueName"],
        durable: true,
        exclusive: false,
        autoDelete: false,
        arguments: arguments
    );

    var properties = new BasicProperties
    {
        DeliveryMode = DeliveryModes.Persistent
    };

    foreach (var m in messageFields)
    {
        var body = Encoding.UTF8.GetBytes(m.Id.ToString());
        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: builder.Configuration.GetSection("RabbitMQ")["QueueName"],
            mandatory: false,
            basicProperties: properties,
            body: body
        );
    }

    var mId = messageFields.Select(m => m.Id);
    return Results.Accepted(value: mId);
});

app.MapPost("api/sms/replay-failed", async (IConfiguration configuration, MyDbContext dbContext, IConnection connection, [FromHeader(Name = "Api-Key")] string? apiKey) =>
{
    var user = await dbContext.Users.FirstOrDefaultAsync(u => u.ApiKey == apiKey);
    if (user is null)
        return Results.Unauthorized();

    var a = await Helper.ReplayFailedMessagesAsync(connection, configuration, dbContext);
    return Results.Ok(a);
});


app.MapControllers();
app.Run();

public record CreateUserRequest(string name);
public record SendSmsRequest([property: JsonConverter(typeof(SingleOrListStringConverter))] List<string> to, string message);
public record SendSmsBulkRequest(List<SmsEnvelopes> Envelopes);