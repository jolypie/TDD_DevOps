using BookLibraryApp.Exceptions;
using BookLibraryApp.Models;

namespace BookLibraryApp.Services;

public class BookService(IBookRepository repository)
{
    public async Task AddBookAsync(string title, string author, int totalPages)
    {
        var existing = await repository.GetByTitleAndAuthorAsync(title, author);
        if (existing != null)
            throw new DuplicateBookException();

        await repository.AddAsync(new Book { Title = title, Author = author, TotalPages = totalPages });
    }
}
