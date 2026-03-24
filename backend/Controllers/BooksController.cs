using BookLibraryApp.Data;
using BookLibraryApp.Exceptions;
using BookLibraryApp.Models;
using BookLibraryApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookLibraryApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController(BookService bookService, AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await db.Books.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var book = await db.Books.FindAsync(id);
        return book is null ? NotFound() : Ok(book);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Author))
            return BadRequest("Title and Author are required.");

        if (request.TotalPages <= 0)
            return BadRequest("TotalPages must be greater than 0.");

        try
        {
            await bookService.AddBookAsync(request.Title, request.Author, request.TotalPages);
            return Ok();
        }
        catch (DuplicateBookException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var book = await db.Books.FindAsync(id);
        if (book is null) return NotFound();
        db.Books.Remove(book);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

public record CreateBookRequest(string Title, string Author, int TotalPages);
