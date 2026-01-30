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
    }
}