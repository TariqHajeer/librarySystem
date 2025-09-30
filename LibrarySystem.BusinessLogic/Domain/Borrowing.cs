using LibrarySystem.BusinessLogic.Domain.Attributes;

namespace LibrarySystem.BusinessLogic.Domain;

public class Borrowing
{
  [IgnoreOnInsert]
  public Guid Id { get; set; }
  [IgnoreOnUpdate]
  public Guid UserId { get; set; }
  [IgnoreOnUpdate]
  public Guid BookId { get; set; }
  [IgnoreOnUpdate]
  public DateTime BorrowDate { get; set; }
  [IgnoreOnInsert]
  public DateTime? ReturnDate { get; set; }
}