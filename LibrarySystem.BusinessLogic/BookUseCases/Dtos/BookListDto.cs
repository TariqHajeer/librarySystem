namespace LibrarySystem.BusinessLogic.BookUseCases.Dtos;

public record class BookListDto : CreateBook
{
    public Guid Id { get; set; }
}
