namespace BookLibraryApp.Models;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public int TotalPages { get; set; }

    public ICollection<ReadingEntry> ReadingEntries { get; set; } = new List<ReadingEntry>();
}
