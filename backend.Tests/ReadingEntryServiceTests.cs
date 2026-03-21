using BookLibraryApp.Exceptions;
using BookLibraryApp.Models;
using BookLibraryApp.Services;
using FluentAssertions;
using Moq;

namespace BookLibraryApp.Tests;

public class ReadingEntryServiceTests
{
    [Fact]
    public async Task AddEntry_WhenEntryForSameBookAlreadyExists_ThrowsDuplicateReadingEntryException()
    {
        // Arrange
        var existingEntry = new ReadingEntry { UserId = 1, BookId = 1 };
        var repositoryMock = new Mock<IReadingEntryRepository>();
        repositoryMock
            .Setup(r => r.GetByUserAndBookAsync(1, 1))
            .ReturnsAsync(existingEntry);

        var service = new ReadingEntryService(repositoryMock.Object);

        // Act
        var act = async () => await service.AddEntryAsync(userId: 1, bookId: 1);

        // Assert
        await act.Should().ThrowAsync<DuplicateReadingEntryException>();
    }

    [Fact]
    public async Task ChangeStatus_WhenSkippingReadingStatus_ThrowsInvalidStatusTransitionException()
    {
        // Arrange
        var book = new Book { Id = 1, TotalPages = 300 };
        var entry = new ReadingEntry { Id = 1, UserId = 1, BookId = 1, Status = ReadingStatus.WantToRead, Book = book };
        var repositoryMock = new Mock<IReadingEntryRepository>();
        repositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(entry);

        var service = new ReadingEntryService(repositoryMock.Object);

        // Act: trying to go WantToRead → Finished (must go through Reading first)
        var act = async () => await service.ChangeStatusAsync(entryId: 1, newStatus: ReadingStatus.Finished);

        // Assert
        await act.Should().ThrowAsync<InvalidStatusTransitionException>();
    }

    [Fact]
    public async Task SetRating_WhenStatusIsNotFinished_ThrowsRatingNotAllowedException()
    {
        // Arrange
        var entry = new ReadingEntry { Id = 1, UserId = 1, BookId = 1, Status = ReadingStatus.Reading };
        var repositoryMock = new Mock<IReadingEntryRepository>();
        repositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(entry);

        var service = new ReadingEntryService(repositoryMock.Object);

        // Act
        var act = async () => await service.SetRatingAsync(entryId: 1, rating: 5);

        // Assert
        await act.Should().ThrowAsync<RatingNotAllowedException>();
    }

    [Fact]
    public async Task UpdateProgress_WhenPagesReadExceedTotalPages_ThrowsPagesExceedTotalException()
    {
        // Arrange
        var book = new Book { Id = 1, TotalPages = 100 };
        var entry = new ReadingEntry { Id = 1, UserId = 1, BookId = 1, Status = ReadingStatus.Reading, Book = book };
        var repositoryMock = new Mock<IReadingEntryRepository>();
        repositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(entry);

        var service = new ReadingEntryService(repositoryMock.Object);

        // Act
        var act = async () => await service.UpdateProgressAsync(entryId: 1, pagesRead: 150);

        // Assert
        await act.Should().ThrowAsync<PagesExceedTotalException>();
    }

    [Fact]
    public async Task SetStartDate_WhenDateIsInTheFuture_ThrowsFutureDateException()
    {
        // Arrange
        var entry = new ReadingEntry { Id = 1, UserId = 1, BookId = 1, Status = ReadingStatus.WantToRead };
        var repositoryMock = new Mock<IReadingEntryRepository>();
        repositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(entry);

        var service = new ReadingEntryService(repositoryMock.Object);
        var futureDate = DateTime.UtcNow.AddDays(1);

        // Act
        var act = async () => await service.SetStartDateAsync(entryId: 1, startDate: futureDate);

        // Assert
        await act.Should().ThrowAsync<FutureDateException>();
    }
}
