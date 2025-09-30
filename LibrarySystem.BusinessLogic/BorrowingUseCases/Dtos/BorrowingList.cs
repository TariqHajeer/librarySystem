using System;

namespace LibrarySystem.BusinessLogic.BorrowingUseCases.Dtos;

public class BorrowingList
{
    public Guid BookId { get; set; }
    public string Title { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; }
}
