namespace BookLibraryApp.Models;

public class ReadingEntry
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int BookId { get; set; }
    public ReadingStatus Status { get; set; } = ReadingStatus.WantToRead;
    public int PagesRead { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? FinishDate { get; set; }
    public int? Rating { get; set; }

    public User User { get; set; } = null!;
    public Book Book { get; set; } = null!;
}
