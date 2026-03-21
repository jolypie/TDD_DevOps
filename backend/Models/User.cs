namespace BookLibraryApp.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public ICollection<ReadingEntry> ReadingEntries { get; set; } = new List<ReadingEntry>();
}
