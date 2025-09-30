using System;
using LibrarySystem.BusinessLogic.BorrowingUseCases.Dtos;
using LibrarySystem.BusinessLogic.Common;
using LibrarySystem.BusinessLogic.Domain;

namespace LibrarySystem.BusinessLogic.BorrowingUseCases;

public interface IBorrowingService : IService<Borrowing, Guid, CreateBorrowing, BorrowingList>
{
    Task ReturnBook(ReturnBook returnBook);
}
