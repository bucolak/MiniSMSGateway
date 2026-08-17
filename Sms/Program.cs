//Console.WriteLine("Hello, World!");

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Sms;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder();

/*var connectionstring = builder.Configuration.GetConnectionString("my-db");

builder.Services.AddDbContext<MyDbContext>(options => options.UseNpgsql(connectionstring));*/

builder.AddNpgsqlDbContext<MyDbContext>("my-db");

builder.Services.AddControllers();

builder.Services.AddHttpClient();

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

app.MapPost("api/sms/send", async (IHttpClientFactory httpClientFactory, MyDbContext dbContext, [FromHeader(Name="Api-Key")] string? apiKey, [FromBody]SendSmsRequest request) =>
{
    var user = await dbContext.Users.FirstOrDefaultAsync(u => u.ApiKey == apiKey);
    if(user is null)
    {
        return Results.Unauthorized();
    }

    var myRequest = new BaseSmsRquest
    {
        Credential = new SmsCredential
        {
            Password = "*********",
            Username = "*********"
        },
        Header = new SmsHeader { },
        Message = request.message,
        To = request.to
    };

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

    
    var (isSuccess, responseBody) = await Helper.SendRequestAsync(httpClientFactory, myRequest);

    if (!isSuccess)
    {
        foreach(var req in messageFields)
            req.Status = Sms.Enum.MessageStatus.Failed;
        await dbContext.SaveChangesAsync();
        return Results.Problem($"Error: {responseBody}!");
    }
    foreach (var req in messageFields)
    {
        req.Status = Sms.Enum.MessageStatus.Sent;
        req.SentAt = DateTime.UtcNow;
    }
       
    await dbContext.SaveChangesAsync();
    return Results.Ok($"Okay!: {responseBody}");
});

app.MapPost("api/sms/send-bulk", async (IHttpClientFactory httpClient, MyDbContext dbContext, [FromHeader(Name = "Api-Key")] string? apiKey, [FromBody] SendSmsBulkRequest request) =>
{
    var user = await dbContext.Users.FirstOrDefaultAsync(u => u.ApiKey == apiKey);
    if (user is null)
        Results.Unauthorized();

    var envelopes = new List<SmsEnvelopes>();

    if (request.Envelopes is null || request.Envelopes.Count == 0)
        return Results.BadRequest("There is no Sms!");

    var myRequest = new BaseSmsBulkRequest
    {
        Credential = new SmsCredential
        {
            Password = "*********",
            Username = "*********"
        },
        Header = new SmsHeader { },
        Envelopes = request.Envelopes
    };

    var messageFields = envelopes.Select(env => new Message
    {
        UserId = user.Id,
        User = user,
        PhoneNumber = env.To,
        Content = env.Message,
        Status = Sms.Enum.MessageStatus.Pending
    }).ToList();

    dbContext.Messages.AddRange(messageFields);
    await dbContext.SaveChangesAsync();

    var (isSuccess, responseBody) = await Helper.SendBulkRequestAsync(httpClient, myRequest);

    if (!isSuccess)
    {
        foreach (var msg in messageFields)
        {
            msg.Status = Sms.Enum.MessageStatus.Failed;
        }
        await dbContext.SaveChangesAsync();
        return Results.Problem("SMS sending process has failed");
    }

    foreach (var msg in messageFields)
    {
        msg.Status = Sms.Enum.MessageStatus.Sent;
        msg.SentAt = DateTime.UtcNow;
    }
    await dbContext.SaveChangesAsync();
    return Results.Ok($"Okay!: {responseBody}");
});

app.MapControllers();
app.Run();

public record CreateUserRequest(string name);
public record SendSmsRequest([property: JsonConverter(typeof(SingleOrListStringConverter))] List<string> to, string message);
public record SendSmsBulkRequest(List<SmsEnvelopes> Envelopes);