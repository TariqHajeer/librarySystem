namespace LibrarySystem.BusinessLogic.BookUseCases.Dtos;

public record class CreateBook
{
    public string Title { get; set; }
    public string Author { get; set; }
    public string ISBN { get; set; }
    public int? PublishedYear { get; set; }
    public int TotalCopies { get; set; } = 1;
    public int AvailableCopies { get; set; } = 1;
}
