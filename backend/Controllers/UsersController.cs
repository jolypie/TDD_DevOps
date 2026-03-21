using BookLibraryApp.Data;
using BookLibraryApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookLibraryApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await db.Users.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await db.Users.FindAsync(id);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpGet("{id}/entries")]
    public async Task<IActionResult> GetUserEntries(int id)
    {
        var entries = await db.ReadingEntries
            .Include(e => e.Book)
            .Where(e => e.UserId == id)
            .ToListAsync();
        return Ok(entries);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] User user)
    {
        if (string.IsNullOrWhiteSpace(user.Name) || string.IsNullOrWhiteSpace(user.Email))
            return BadRequest("Name and Email are required.");

        db.Users.Add(user);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }
}
