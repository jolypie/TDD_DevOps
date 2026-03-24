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

    [Fact]
    public async Task AddEntry_WhenNoPreviousEntry_CallsRepositoryAdd()
    {
        // Arrange
        var repositoryMock = new Mock<IReadingEntryRepository>();
        repositoryMock
            .Setup(r => r.GetByUserAndBookAsync(1, 1))
            .ReturnsAsync((ReadingEntry?)null);

        var service = new ReadingEntryService(repositoryMock.Object);

        // Act
        await service.AddEntryAsync(userId: 1, bookId: 1);

        // Assert
        repositoryMock.Verify(r => r.AddAsync(It.Is<ReadingEntry>(e =>
            e.UserId == 1 && e.BookId == 1 && e.Status == ReadingStatus.WantToRead)), Times.Once);
    }

    [Fact]
    public async Task ChangeStatus_WhenValid_UpdatesEntryStatus()
    {
        // Arrange
        var book = new Book { Id = 1, TotalPages = 200 };
        var entry = new ReadingEntry { Id = 1, UserId = 1, BookId = 1, Status = ReadingStatus.WantToRead, Book = book };
        var repositoryMock = new Mock<IReadingEntryRepository>();
        repositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(entry);

        var service = new ReadingEntryService(repositoryMock.Object);

        // Act
        await service.ChangeStatusAsync(entryId: 1, newStatus: ReadingStatus.Reading);

        // Assert
        entry.Status.Should().Be(ReadingStatus.Reading);
        repositoryMock.Verify(r => r.UpdateAsync(entry), Times.Once);
    }

    [Fact]
    public async Task ChangeStatus_WhenFinished_AutoSetsPagesReadToTotalPages()
    {
        // Arrange
        var book = new Book { Id = 1, TotalPages = 350 };
        var entry = new ReadingEntry { Id = 1, UserId = 1, BookId = 1, Status = ReadingStatus.Reading, Book = book, PagesRead = 100, StartDate = DateTime.UtcNow.AddDays(-5) };
        var repositoryMock = new Mock<IReadingEntryRepository>();
        repositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(entry);

        var service = new ReadingEntryService(repositoryMock.Object);

        // Act
        await service.ChangeStatusAsync(entryId: 1, newStatus: ReadingStatus.Finished);

        // Assert
        entry.PagesRead.Should().Be(350);
        entry.Status.Should().Be(ReadingStatus.Finished);
    }

    [Fact]
    public async Task SetRating_WhenStatusIsFinished_UpdatesRating()
    {
        // Arrange
        var entry = new ReadingEntry { Id = 1, UserId = 1, BookId = 1, Status = ReadingStatus.Finished };
        var repositoryMock = new Mock<IReadingEntryRepository>();
        repositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(entry);

        var service = new ReadingEntryService(repositoryMock.Object);

        // Act
        await service.SetRatingAsync(entryId: 1, rating: 4);

        // Assert
        entry.Rating.Should().Be(4);
        repositoryMock.Verify(r => r.UpdateAsync(entry), Times.Once);
    }

    [Fact]
    public async Task UpdateProgress_WhenValid_UpdatesPagesRead()
    {
        // Arrange
        var book = new Book { Id = 1, TotalPages = 300 };
        var entry = new ReadingEntry { Id = 1, UserId = 1, BookId = 1, Status = ReadingStatus.Reading, Book = book };
        var repositoryMock = new Mock<IReadingEntryRepository>();
        repositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(entry);

        var service = new ReadingEntryService(repositoryMock.Object);

        // Act
        await service.UpdateProgressAsync(entryId: 1, pagesRead: 120);

        // Assert
        entry.PagesRead.Should().Be(120);
        repositoryMock.Verify(r => r.UpdateAsync(entry), Times.Once);
    }

    [Fact]
    public async Task SetStartDate_WhenValid_UpdatesStartDate()
    {
        // Arrange
        var entry = new ReadingEntry { Id = 1, UserId = 1, BookId = 1, Status = ReadingStatus.Reading };
        var repositoryMock = new Mock<IReadingEntryRepository>();
        repositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(entry);

        var service = new ReadingEntryService(repositoryMock.Object);
        var yesterday = DateTime.UtcNow.AddDays(-1);

        // Act
        await service.SetStartDateAsync(entryId: 1, startDate: yesterday);

        // Assert
        entry.StartDate.Should().Be(yesterday);
        repositoryMock.Verify(r => r.UpdateAsync(entry), Times.Once);
    }

    [Fact]
    public async Task ChangeStatus_WhenFinishedWithoutStartDate_ThrowsStartDateRequiredException()
    {
        // Arrange
        var book = new Book { Id = 1, TotalPages = 200 };
        var entry = new ReadingEntry { Id = 1, UserId = 1, BookId = 1, Status = ReadingStatus.Reading, Book = book, StartDate = null };
        var repositoryMock = new Mock<IReadingEntryRepository>();
        repositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(entry);

        var service = new ReadingEntryService(repositoryMock.Object);

        // Act
        var act = async () => await service.ChangeStatusAsync(entryId: 1, newStatus: ReadingStatus.Finished);

        // Assert
        await act.Should().ThrowAsync<StartDateRequiredException>();
    }

    [Fact]
    public async Task SetFinishDate_WhenDateIsInTheFuture_ThrowsFutureDateNotAllowedException()
    {
        // Arrange
        var entry = new ReadingEntry { Id = 1, UserId = 1, BookId = 1, Status = ReadingStatus.Finished };
        var repositoryMock = new Mock<IReadingEntryRepository>();
        repositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(entry);

        var service = new ReadingEntryService(repositoryMock.Object);

        // Act
        var act = async () => await service.SetFinishDateAsync(entryId: 1, finishDate: DateTime.UtcNow.AddDays(5));

        // Assert
        await act.Should().ThrowAsync<FutureDateNotAllowedException>();
    }

    [Fact]
    public async Task SetStartDate_WhenStartDateIsAfterExistingFinishDate_ThrowsFinishDateBeforeStartDateException()
    {
        // Arrange
        var finishDate = DateTime.UtcNow.AddDays(-2);
        var entry = new ReadingEntry
        {
            Id = 1, UserId = 1, BookId = 1,
            Status = ReadingStatus.Finished,
            FinishDate = finishDate
        };
        var repositoryMock = new Mock<IReadingEntryRepository>();
        repositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(entry);

        var service = new ReadingEntryService(repositoryMock.Object);

        // Act: start date is after finish date
        var act = async () => await service.SetStartDateAsync(entryId: 1, startDate: finishDate.AddDays(1));

        // Assert
        await act.Should().ThrowAsync<FinishDateBeforeStartDateException>();
    }

    [Fact]
    public async Task SetFinishDate_WhenFinishDateIsBeforeStartDate_ThrowsFinishDateBeforeStartDateException()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-10);
        var entry = new ReadingEntry
        {
            Id = 1, UserId = 1, BookId = 1,
            Status = ReadingStatus.Finished,
            StartDate = startDate
        };
        var repositoryMock = new Mock<IReadingEntryRepository>();
        repositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(entry);

        var service = new ReadingEntryService(repositoryMock.Object);

        // Act: finish date is 5 days before start date
        var act = async () => await service.SetFinishDateAsync(entryId: 1, finishDate: startDate.AddDays(-5));

        // Assert
        await act.Should().ThrowAsync<FinishDateBeforeStartDateException>();
    }
}
