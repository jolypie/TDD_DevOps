using BookLibraryApp.Exceptions;
using BookLibraryApp.Models;
using BookLibraryApp.Services;
using FluentAssertions;
using Moq;

namespace BookLibraryApp.Tests;

public class BookServiceTests
{
    [Fact]
    public async Task AddBook_WhenBookWithSameTitleAndAuthorExists_ThrowsDuplicateBookException()
    {
        // Arrange
        var existingBook = new Book { Id = 1, Title = "Clean Code", Author = "Martin", TotalPages = 300 };
        var repositoryMock = new Mock<IBookRepository>();
        repositoryMock
            .Setup(r => r.GetByTitleAndAuthorAsync("Clean Code", "Martin"))
            .ReturnsAsync(existingBook);

        var service = new BookService(repositoryMock.Object);

        // Act
        var act = async () => await service.AddBookAsync("Clean Code", "Martin", totalPages: 300);

        // Assert
        await act.Should().ThrowAsync<DuplicateBookException>();
    }
}
