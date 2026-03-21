using BookLibraryApp.Models;

namespace BookLibraryApp.Services;

public interface IReadingEntryRepository
{
    Task<ReadingEntry?> GetByUserAndBookAsync(int userId, int bookId);
    Task AddAsync(ReadingEntry entry);
}
