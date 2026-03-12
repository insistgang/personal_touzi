using Microsoft.EntityFrameworkCore;
using PersonalTouzi.Core.Entities;

namespace PersonalTouzi.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Position> Positions { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<AssetSnapshot> AssetSnapshots { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
