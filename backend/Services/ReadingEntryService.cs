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

    public async Task ChangeStatusAsync(int entryId, ReadingStatus newStatus)
    {
        var entry = await _repository.GetByIdAsync(entryId);
        if (entry == null) throw new KeyNotFoundException("Reading entry not found.");

        if (newStatus <= entry.Status)
            throw new InvalidStatusTransitionException(entry.Status, newStatus);

        entry.Status = newStatus;
        await _repository.UpdateAsync(entry);
    }

    public async Task SetRatingAsync(int entryId, int rating)
    {
        var entry = await _repository.GetByIdAsync(entryId);
        if (entry == null) throw new KeyNotFoundException("Reading entry not found.");

        if (entry.Status != ReadingStatus.Finished)
            throw new RatingNotAllowedException();

        entry.Rating = rating;
        await _repository.UpdateAsync(entry);
    }

    public async Task UpdateProgressAsync(int entryId, int pagesRead)
    {
        var entry = await _repository.GetByIdAsync(entryId);
        if (entry == null) throw new KeyNotFoundException("Reading entry not found.");

        if (pagesRead > entry.Book.TotalPages)
            throw new PagesExceedTotalException();

        entry.PagesRead = pagesRead;
        await _repository.UpdateAsync(entry);
    }
}
