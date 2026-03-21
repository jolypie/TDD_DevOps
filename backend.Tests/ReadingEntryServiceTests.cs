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
}
