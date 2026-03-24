using BookLibraryApp.Data;
using BookLibraryApp.Exceptions;
using BookLibraryApp.Models;
using BookLibraryApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookLibraryApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReadingEntriesController(ReadingEntryService service, AppDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> AddEntry([FromBody] AddEntryRequest request)
    {
        try
        {
            await service.AddEntryAsync(request.UserId, request.BookId);
            return Ok();
        }
        catch (DuplicateReadingEntryException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeStatusRequest request)
    {
        try
        {
            await service.ChangeStatusAsync(id, request.Status);
            return NoContent();
        }
        catch (InvalidStatusTransitionException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPatch("{id}/rating")]
    public async Task<IActionResult> SetRating(int id, [FromBody] SetRatingRequest request)
    {
        try
        {
            await service.SetRatingAsync(id, request.Rating);
            return NoContent();
        }
        catch (RatingNotAllowedException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPatch("{id}/progress")]
    public async Task<IActionResult> UpdateProgress(int id, [FromBody] UpdateProgressRequest request)
    {
        try
        {
            await service.UpdateProgressAsync(id, request.PagesRead);
            return NoContent();
        }
        catch (PagesExceedTotalException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPatch("{id}/start-date")]
    public async Task<IActionResult> SetStartDate(int id, [FromBody] SetStartDateRequest request)
    {
        try
        {
            await service.SetStartDateAsync(id, request.StartDate);
            return NoContent();
        }
        catch (FutureDateException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPatch("{id}/finish-date")]
    public async Task<IActionResult> SetFinishDate(int id, [FromBody] SetFinishDateRequest request)
    {
        try
        {
            await service.SetFinishDateAsync(id, request.FinishDate);
            return NoContent();
        }
        catch (FutureDateNotAllowedException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (FinishDateBeforeStartDateException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEntry(int id)
    {
        var entry = await db.ReadingEntries.FindAsync(id);
        if (entry == null) return NotFound();
        db.ReadingEntries.Remove(entry);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

public record AddEntryRequest(int UserId, int BookId);
public record ChangeStatusRequest(ReadingStatus Status);
public record SetRatingRequest(int Rating);
public record UpdateProgressRequest(int PagesRead);
public record SetStartDateRequest(DateTime StartDate);
public record SetFinishDateRequest(DateTime FinishDate);
