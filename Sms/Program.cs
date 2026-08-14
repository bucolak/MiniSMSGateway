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

    dbContext.Database.EnsureCreated();

    dbContext.Users.Add(new User {Name = "Buket" });
    dbContext.Users.Add(new User {Name = "Selin" });
    dbContext.SaveChanges();

    var userlist = dbContext.Users.ToList();

    foreach (var user in userlist)
        Console.WriteLine($"ID: {user.Id} - Name: {user.Name}");
}

app.MapGet("/api/users", (MyDbContext dbContext) =>
    {
        return Results.Ok(dbContext.Users.ToList());
    });

//app.MapGet("api/send", async (IHttpClientFactory httpClientFactory) =>
//{

//    //var baseRequest = new BaseSmsRquest
//    //{
//    //    Credential = new SmsCredential
//    //    {
//    //        Password = "C8aykU*GX8",
//    //        Username = "balaban-bulktest"
//    //    },

//    //    Header = new SmsHeader { },

//    //    Message = "10-Buket Last Demo",
//    //    To = "905307869075"
//    //};

//    //var client = new HttpClient();

//    //var response = await client.PostAsJsonAsync(
//    //    "https://sgw.maradit.net/api/json/reply/Submit",
//    //    baseRequest
//    //);

//    //Console.WriteLine(response.StatusCode);




//    //var client = new HttpClient();

//    //var request = new HttpRequestMessage(HttpMethod.Post, "https://sgw.maradit.net/api/json/reply/Submit");

//    //var baseRequest = new BaseSmsRquest
//    //{
//    //    Credential = new SmsCredential
//    //    {
//    //        Password = "C8aykU*GX8",
//    //        Username = "balaban-bulktest"
//    //    },
//    //    Header = new SmsHeader { },
//    //    Message = "10-Buket Last Demo",
//    //    To = "905307869075"
//    //};


//    //var baseRequest = new BaseSmsRquest
//    //{
//    //    Credential = new SmsCredential
//    //    {
//    //        Password = "*****",
//    //        Username = "*****"
//    //    },
//    //    Header = new SmsHeader { },
//    //    Message = "10-Buket Last Demo",
//    //    To = "905307*****"
//    //};


//    //var jsonBody = System.Text.Json.JsonSerializer.Serialize(baseRequest);

//    //var content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");

//    //request.Content = content;

//    //var response = await client.PostAsJsonAsync()

//    //response.EnsureSuccessStatusCode();

//    //Console.WriteLine(await response.Content.ReadAsStringAsync());


//    var client = httpClientFactory.CreateClient();

//    var request = new BaseSmsRquest
//    {
//        Credential = new SmsCredential
//        {
//            Password = "C8aykU*GX8",
//            Username = "balaban-bulktest"
//        },
//        Header = new SmsHeader { },
//        Message = "10-Buket Last Demo",
//        To = "905307869075"
//    };

//    var jsonOptions = new System.Text.Json.JsonSerializerOptions
//    {
//        PropertyNamingPolicy = null
//    };

//    var jsonBody = System.Text.Json.JsonSerializer.Serialize(request, jsonOptions);

//    using var content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");



//    var response = await client.PostAsJsonAsync("https://sgw.maradit.net/api/json/reply/Submit", jsonBody);

//    if (!response.IsSuccessStatusCode)
//    {
//        var resb = await response.Content.ReadAsStringAsync();
//        return Results.Problem($"There is an error!: {resb}");
//    }
//    var reso = await response.Content.ReadAsStringAsync();
//    return Results.Ok($"Okay!: {reso}");

//});

app.MapGet("api/send", async (IHttpClientFactory httpClientFactory) =>
{
    var (isSuccess, responseBody) = await Helper.SendRequestAsync(httpClientFactory);

    if(!isSuccess)
        return Results.Problem($"Error: {responseBody}!");
    return Results.Ok($"Okay!: {responseBody}");
});

app.MapControllers();
app.Run();