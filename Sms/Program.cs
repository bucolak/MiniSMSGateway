//Console.WriteLine("Hello, World!");

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sms;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

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

app.MapGet("/api/user", (MyDbContext dbContext, string name) =>
{
    var request = new User
    {
        Name = name,
        ApiKey = Guid.NewGuid().ToString()
    };
    dbContext.Users.Add(request);
    dbContext.SaveChanges();
    return Results.Created($"api/send/{request.Id}", request);
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
    return Results.Ok(dbContext.Messages.ToList());
});

app.MapGet("/api/sms/status/{id}", async (MyDbContext dbContext,int id) =>
{
    var message = await dbContext.Messages.FirstOrDefaultAsync(m => m.Id == id);
    if (message is null)
        return Results.NotFound($"SMS record with ID {id} not found!");
    return Results.Ok(message);
});

app.MapGet("api/sms/send", async (IHttpClientFactory httpClientFactory, MyDbContext dbContext, [FromHeader(Name="Api-Key")] string? apiKey, string to, string message) =>
{
    var user = await dbContext.Users.FirstOrDefaultAsync(u => u.ApiKey == apiKey);
    if(user is null)
    {
        return Results.Unauthorized();
    }

    var messageFields = new Message
    {
        UserId = user.Id,
        User = user,
        PhoneNumber = to,
        Content = message,
        Status = Sms.Enum.MessageStatus.Pending
    };

    dbContext.Messages.Add(messageFields);
    await dbContext.SaveChangesAsync();

    var request = new BaseSmsRquest
    {
        Credential = new SmsCredential
        {
            Password = "C8aykU*GX8",
            Username = "balaban-bulktest"
        },
        Header = new SmsHeader { },
        Message = message,
        To = to
    };
    var (isSuccess, responseBody) = await Helper.SendRequestAsync(httpClientFactory, request);

    if (!isSuccess)
    {
        messageFields.Status = Sms.Enum.MessageStatus.Failed;
        await dbContext.SaveChangesAsync();
        return Results.Problem($"Error: {responseBody}!");
    }

    messageFields.Status = Sms.Enum.MessageStatus.Sent;
    messageFields.SentAt = DateTime.UtcNow;
    await dbContext.SaveChangesAsync();
    return Results.Ok($"Okay!: {responseBody}");
});

app.MapControllers();
app.Run();