using BookLibraryApp.Models;
using BookLibraryApp.Services;
using Microsoft.EntityFrameworkCore;

namespace BookLibraryApp.Data;

public class EfBookRepository(AppDbContext db) : IBookRepository
{
    public async Task<Book?> GetByTitleAndAuthorAsync(string title, string author) =>
        await db.Books.FirstOrDefaultAsync(b =>
            b.Title == title && b.Author == author);

    public async Task AddAsync(Book book)
    {
        db.Books.Add(book);
        await db.SaveChangesAsync();
    }
}
