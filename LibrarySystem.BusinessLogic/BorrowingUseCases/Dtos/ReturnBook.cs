using System;

namespace LibrarySystem.BusinessLogic.BorrowingUseCases.Dtos;

public class ReturnBook
{
    public Guid BookId { get; set; }
    public Guid UserId { get; set; }
    public DateTime ReturnDate { get; set; }
}
