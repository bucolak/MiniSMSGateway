using Microsoft.EntityFrameworkCore;
using MiniSMSGateway.ApiService.Models;

namespace MiniSMSGateway.ApiService.Data;

public class SmsDbContext : DbContext
{
    public SmsDbContext(DbContextOptions<SmsDbContext> options)
        : base(options) {}

    public DbSet<User> Users { get; set; }
    public DbSet<Message> Messages { get; set; }
}

