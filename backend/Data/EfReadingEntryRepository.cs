using BookLibraryApp.Models;
using BookLibraryApp.Services;
using Microsoft.EntityFrameworkCore;

namespace BookLibraryApp.Data;

public class EfReadingEntryRepository(AppDbContext db) : IReadingEntryRepository
{
    public async Task<ReadingEntry?> GetByUserAndBookAsync(int userId, int bookId) =>
        await db.ReadingEntries
            .FirstOrDefaultAsync(e => e.UserId == userId && e.BookId == bookId);

    public async Task<ReadingEntry?> GetByIdAsync(int id) =>
        await db.ReadingEntries
            .Include(e => e.Book)
            .FirstOrDefaultAsync(e => e.Id == id);

    public async Task AddAsync(ReadingEntry entry)
    {
        db.ReadingEntries.Add(entry);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(ReadingEntry entry)
    {
        db.ReadingEntries.Update(entry);
        await db.SaveChangesAsync();
    }
}
