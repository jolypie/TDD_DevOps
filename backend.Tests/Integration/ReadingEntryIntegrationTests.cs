using BookLibraryApp.Data;
using BookLibraryApp.Exceptions;
using BookLibraryApp.Models;
using BookLibraryApp.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BookLibraryApp.Tests.Integration;

public class ReadingEntryIntegrationTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly ReadingEntryService _service;

    public ReadingEntryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        var repository = new EfReadingEntryRepository(_db);
        _service = new ReadingEntryService(repository);
    }

    [Fact]
    public async Task AddEntry_WhenValid_SavesEntryToDatabase()
    {
        var user = new User { Name = "Alice", Email = "alice@example.com" };
        var book = new Book { Title = "Clean Code", Author = "Martin", TotalPages = 300 };
        _db.Users.Add(user);
        _db.Books.Add(book);
        await _db.SaveChangesAsync();

        await _service.AddEntryAsync(user.Id, book.Id);

        var entry = await _db.ReadingEntries.FirstOrDefaultAsync();
        entry.Should().NotBeNull();
        entry!.UserId.Should().Be(user.Id);
        entry.BookId.Should().Be(book.Id);
        entry.Status.Should().Be(ReadingStatus.WantToRead);
    }

    [Fact]
    public async Task AddEntry_WhenDuplicate_ThrowsAndDoesNotSave()
    {
        var user = new User { Name = "Bob", Email = "bob@example.com" };
        var book = new Book { Title = "DDIA", Author = "Kleppmann", TotalPages = 400 };
        _db.Users.Add(user);
        _db.Books.Add(book);
        await _db.SaveChangesAsync();

        await _service.AddEntryAsync(user.Id, book.Id);

        var act = async () => await _service.AddEntryAsync(user.Id, book.Id);

        await act.Should().ThrowAsync<DuplicateReadingEntryException>();
        _db.ReadingEntries.Count().Should().Be(1);
    }

    [Fact]
    public async Task ChangeStatus_WhenValid_UpdatesStatusInDatabase()
    {
        var user = new User { Name = "Carol", Email = "carol@example.com" };
        var book = new Book { Title = "The Pragmatic Programmer", Author = "Hunt", TotalPages = 352 };
        _db.Users.Add(user);
        _db.Books.Add(book);
        await _db.SaveChangesAsync();

        await _service.AddEntryAsync(user.Id, book.Id);
        var entry = await _db.ReadingEntries.FirstAsync();

        await _service.ChangeStatusAsync(entry.Id, ReadingStatus.Reading);

        var updated = await _db.ReadingEntries.FindAsync(entry.Id);
        updated!.Status.Should().Be(ReadingStatus.Reading);
    }

    public void Dispose() => _db.Dispose();
}
