using BookLibraryApp.Data;
using BookLibraryApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookLibraryApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController(AppDbContext db) : ControllerBase
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
    public async Task<IActionResult> Create([FromBody] Book book)
    {
        if (string.IsNullOrWhiteSpace(book.Title) || string.IsNullOrWhiteSpace(book.Author))
            return BadRequest("Title and Author are required.");

        if (book.TotalPages <= 0)
            return BadRequest("TotalPages must be greater than 0.");

        db.Books.Add(book);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
    }
}
