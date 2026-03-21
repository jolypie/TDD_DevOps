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
        var entry = await GetEntryOrThrowAsync(entryId);

        if (entry.Status == ReadingStatus.WantToRead && newStatus == ReadingStatus.Finished)
            throw new InvalidStatusTransitionException(entry.Status, newStatus);

        entry.Status = newStatus;

        if (newStatus == ReadingStatus.Finished)
            entry.PagesRead = entry.Book.TotalPages;

        await _repository.UpdateAsync(entry);
    }

    public async Task SetRatingAsync(int entryId, int rating)
    {
        var entry = await GetEntryOrThrowAsync(entryId);

        if (entry.Status != ReadingStatus.Finished)
            throw new RatingNotAllowedException();

        entry.Rating = rating;
        await _repository.UpdateAsync(entry);
    }

    public async Task UpdateProgressAsync(int entryId, int pagesRead)
    {
        var entry = await GetEntryOrThrowAsync(entryId);

        if (pagesRead > entry.Book.TotalPages)
            throw new PagesExceedTotalException();

        entry.PagesRead = pagesRead;
        await _repository.UpdateAsync(entry);
    }

    public async Task SetStartDateAsync(int entryId, DateTime startDate)
    {
        if (startDate > DateTime.UtcNow)
            throw new FutureDateException();

        var entry = await GetEntryOrThrowAsync(entryId);
        entry.StartDate = startDate;
        await _repository.UpdateAsync(entry);
    }

    private async Task<ReadingEntry> GetEntryOrThrowAsync(int entryId)
    {
        var entry = await _repository.GetByIdAsync(entryId);
        if (entry == null) throw new KeyNotFoundException($"Reading entry {entryId} not found.");
        return entry;
    }
}
