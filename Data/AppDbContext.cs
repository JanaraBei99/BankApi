using BankApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Client> Clients { get; set; }

    public DbSet<Phone> Phones { get; set; }

    public DbSet<Account> Accounts { get; set; }

    public DbSet<Card> Cards { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Phone>()
            .HasOne<Client>()
            .WithMany()
            .HasForeignKey(phone => phone.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Account>()
            .HasOne<Client>()
            .WithMany()
            .HasForeignKey(account => account.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Card>()
            .HasOne<Account>()
            .WithMany()
            .HasForeignKey(card => card.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Card>()
            .HasIndex(card => card.CardNumber)
            .IsUnique();
    }
}