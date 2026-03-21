using BookLibraryApp.Models;

namespace BookLibraryApp.Services;

public interface IReadingEntryRepository
{
    Task<ReadingEntry?> GetByUserAndBookAsync(int userId, int bookId);
    Task<ReadingEntry?> GetByIdAsync(int id);
    Task AddAsync(ReadingEntry entry);
    Task UpdateAsync(ReadingEntry entry);
}
