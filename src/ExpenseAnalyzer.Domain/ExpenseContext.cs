using Microsoft.EntityFrameworkCore;

namespace ExpenseAnalyzer.Domain;

public class ExpenseContext : DbContext
{
    public ExpenseContext(DbContextOptions<ExpenseContext> options) : base(options)
    {

    }


    public DbSet<Category> Category { get; set; }
    public DbSet<Account> Account { get; set; }
    public DbSet<Tag> Tag { get; set; }
    public DbSet<Expense> Expense { get; set; }
    public DbSet<ExpenseTag> ExpenseTag { get; set; }
    public DbSet<Income> Income { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLazyLoadingProxies();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<Category>()
            .HasIndex(x => x.Name)
            .IsUnique();

        modelBuilder.Entity<Account>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<Account>()
            .HasIndex(x => x.Name)
            .IsUnique();

        modelBuilder.Entity<Tag>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<Tag>()
            .HasIndex(x => x.Name)
            .IsUnique();

        modelBuilder.Entity<Expense>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<ExpenseTag>()
            .HasKey(x => new { x.ExpenseId, x.TagId });

        modelBuilder.Entity<ExpenseTag>()
            .HasOne(et => et.Expense)
            .WithMany(e => e.ExpenseTags)
            .HasForeignKey(et => et.ExpenseId);

        modelBuilder.Entity<ExpenseTag>()
            .HasOne(et => et.Tag)
            .WithMany()
            .HasForeignKey(et => et.TagId);

        // Income: Primary Key
        modelBuilder.Entity<Income>()
            .HasKey(x => x.Id);

        // Income: Explicit decimal precision (standard currency format)
        modelBuilder.Entity<Income>()
            .Property(i => i.Amount)
            .HasColumnType("decimal(18,2)");

        // Income → Category: FK relationship with Restrict delete
        modelBuilder.Entity<Income>()
            .HasOne(i => i.Category)
            .WithMany()
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Income → Account: FK relationship with Restrict delete
        modelBuilder.Entity<Income>()
            .HasOne(i => i.Account)
            .WithMany()
            .HasForeignKey(i => i.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // Income: Indexes for query performance
        modelBuilder.Entity<Income>()
            .HasIndex(i => i.Timestamp);

        modelBuilder.Entity<Income>()
            .HasIndex(i => i.AccountId);

        modelBuilder.Entity<Income>()
            .HasIndex(i => i.CategoryId);
    }
}