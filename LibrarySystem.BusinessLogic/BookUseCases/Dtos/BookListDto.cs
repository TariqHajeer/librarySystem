namespace LibrarySystem.BusinessLogic.BookUseCases.Dtos;

public record class BookListDto : CreateBook
{
    public Guid BookId { get; set; }
}
