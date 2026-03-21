using BookLibraryApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BookLibraryApp.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<ReadingEntry> ReadingEntries => Set<ReadingEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReadingEntry>()
            .HasOne(e => e.User)
            .WithMany(u => u.ReadingEntries)
            .HasForeignKey(e => e.UserId);

        modelBuilder.Entity<ReadingEntry>()
            .HasOne(e => e.Book)
            .WithMany(b => b.ReadingEntries)
            .HasForeignKey(e => e.BookId);

        modelBuilder.Entity<ReadingEntry>()
            .HasIndex(e => new { e.UserId, e.BookId })
            .IsUnique();

        modelBuilder.Entity<ReadingEntry>()
            .Property(e => e.Status)
            .HasConversion<string>();
    }
}
