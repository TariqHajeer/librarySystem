namespace LibrarySystem.BusinessLogic.BorrowingUseCases.Dtos;

public record class CreateBorrowing
{
    public Guid UserId { get; set; }
    public Guid BookId { get; set; }
    public DateTime BorrowDate { get; set; }
}
