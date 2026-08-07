using Maradit;
using Microsoft.EntityFrameworkCore;
using MiniSMSGateway.ApiService.Data;
using MiniSMSGateway.ApiService.Providers;
using MiniSMSGateway.ApiService.Providers.Maradit;
using MiniSMSGateway.ApiService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISmsService, SmsService>();

builder.Services.AddSingleton<Messenger>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var username = config["Maradit:Username"]
        ?? throw new InvalidOperationException("Maradit:Username is not found in the config!");
    var password = config["Maradit:Password"]
        ?? throw new InvalidOperationException("Maradit:Password is not found in the config!");
    return new Messenger(username, password);
});
builder.Services.AddScoped<ISmsProvider, MaraditSdkProvider>();
/*if (builder.Configuration["Maradit:ProviderType"] == "Http")
    builder.Services.AddScoped<ISmsProvider, MaraditHttpProvider>();
else
{
    builder.Services.AddSingleton<Messenger>(sp =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        var username = config["Maradit:Username"]
            ?? throw new InvalidOperationException("Maradit:Username is not found in the config!");
        var password = config["Maradit:Password"]
            ?? throw new InvalidOperationException("Maradit:Password is not found in the config!");
        return new Messenger(username, password);
    });
    builder.Services.AddScoped<ISmsProvider, MaraditSdkProvider>();
}*/

builder.Services.AddDbContext<SmsDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("SmsDb"));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SmsDbContext>();

    var retries = 5;
    while (true)
    {
        try
        {
            db.Database.Migrate();
            break;
        }
        catch (Exception ex) when (retries > 0 && ex is Npgsql.NpgsqlException or Npgsql.PostgresException)
        {
            // "database already exists" - Aspire'ın kendi oluşturma işlemiyle yarıştık,
            // veritabanı artık var, kısa bekleyip tekrar dene
            retries--;
            Thread.Sleep(1000);
        }
    }
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapDefaultEndpoints();

app.MapControllers();

app.Run();
