using BookLibraryApp.Models;

namespace BookLibraryApp.Exceptions;

public class DuplicateBookException : Exception
{
    public DuplicateBookException()
        : base("A book with the same title and author already exists.") { }
}

public class DuplicateReadingEntryException : Exception
{
    public DuplicateReadingEntryException()
        : base("User already has this book in their reading list.") { }
}

public class InvalidStatusTransitionException : Exception
{
    public InvalidStatusTransitionException(ReadingStatus from, ReadingStatus to)
        : base($"Cannot transition status from {from} to {to}.") { }
}

public class RatingNotAllowedException : Exception
{
    public RatingNotAllowedException()
        : base("Rating can only be set when status is Finished.") { }
}

public class PagesExceedTotalException : Exception
{
    public PagesExceedTotalException()
        : base("Pages read cannot exceed total pages of the book.") { }
}

public class FutureDateException : Exception
{
    public FutureDateException()
        : base("Start date cannot be in the future.") { }
}

public class FutureDateNotAllowedException : Exception
{
    public FutureDateNotAllowedException(string fieldName)
        : base($"{fieldName} cannot be a future date.") { }
}

public class StartDateRequiredException : Exception
{
    public StartDateRequiredException()
        : base("StartDate is required before marking the book as Finished.") { }
}

public class FinishDateBeforeStartDateException : Exception
{
    public FinishDateBeforeStartDateException()
        : base("Finish date cannot be earlier than start date.") { }
}
