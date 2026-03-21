using BookLibraryApp.Exceptions;
using BookLibraryApp.Models;

namespace BookLibraryApp.Services;

public class ReadingEntryService
{
    private readonly IReadingEntryRepository _repository;

    public ReadingEntryService(IReadingEntryRepository repository)
    {
        _repository = repository;
    }

    public async Task AddEntryAsync(int userId, int bookId)
    {
        var existing = await _repository.GetByUserAndBookAsync(userId, bookId);
        if (existing != null)
            throw new DuplicateReadingEntryException();

        await _repository.AddAsync(new ReadingEntry { UserId = userId, BookId = bookId });
    }
}
