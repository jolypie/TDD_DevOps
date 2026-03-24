using BookLibraryApp.Models;

namespace BookLibraryApp.Services;

public interface IBookRepository
{
    Task<Book?> GetByTitleAndAuthorAsync(string title, string author);
    Task AddAsync(Book book);
}
