using LibrarySystem.BusinessLogic.BorrowingUseCases.Dtos;
using LibrarySystem.BusinessLogic.Common;
using LibrarySystem.BusinessLogic.Domain;
using LibrarySystem.BusinessLogic.Repos.Interfaces;
using LibrarySystem.DataAccess.Exceptions;
using RedisLocking;

namespace LibrarySystem.BusinessLogic.BorrowingUseCases;

internal class BorrowingService : Service<Borrowing, Guid, CreateBorrowing, BorrowingList>, IBorrowingService
{
    private readonly ILockService _lockService;
    private readonly IRepository<Book, Guid> _bookRepo;
    private readonly IRepository<User, Guid> _userRepo;
    public BorrowingService(IRepository<Borrowing, Guid> repository, IRepository<Book, Guid> bookRepo, IRepository<User, Guid> userRepo, ILockService lockService) : base(repository)
    {
        _lockService = lockService;
        _bookRepo = bookRepo;
        _userRepo = userRepo;
    }
    public override async Task<Guid> Create(CreateBorrowing create)
{
    var lockKey = GetBorrowKey(create.BookId);
    var user = await _userRepo.GetById(create.UserId) 
               ?? throw new ConflictException("User does not exist.");
    return await ExecuteWithRetry(async () =>
    {
        var lockAcquired = await _lockService.RunWithLockAsync(lockKey, TimeSpan.FromSeconds(10), async () =>
        {
            var book = await _bookRepo.GetById(create.BookId) ?? throw new ConflictException($"No book exists with ID {create.BookId}.");
            if (book.AvailableCopies <= 0)
                throw new ConflictException("No available copies.");

            var data = await _repository.Get(new Dictionary<string, object>()
            {
                { nameof(Borrowing.UserId), create.UserId },
                { nameof(Borrowing.BookId), create.BookId },
                { nameof(Borrowing.ReturnDate), null }
            }, 0, 1);

            if (data.TotalCount == 1)
                throw new ConflictException("You already have this book.");

            await _repository.BeginTransaction(true);

            try
            {
                var id = await _repository.Insert(new Borrowing
                {
                    BookId = create.BookId,
                    UserId = create.UserId,
                    BorrowDate = create.BorrowDate
                });

                book.AvailableCopies--;
                await _bookRepo.Update(book);

                await _repository.CommitTransaction();
                return id;
            }
            catch
            {
                await _repository.RollbackTransaction();
                throw;
            }
        });
        return lockAcquired;
    });
}



    public async Task ReturnBook(ReturnBook returnBook)
{
    var lockKey = GetBorrowKey(returnBook.BookId);
    var user = await _userRepo.GetById(returnBook.UserId) 
               ?? throw new ConflictException("User does not exist.");

    await ExecuteWithRetry(async () =>
    {
        // Using the new lock service with RunWithLockAsync
        var lockAcquired = await _lockService.RunWithLockAsync(lockKey, TimeSpan.FromSeconds(10), async () =>
        {
            var book = await _bookRepo.GetById(returnBook.BookId);
            if (book == null)
                throw new ConflictException($"No book exists with ID {returnBook.BookId}.");

            var data = await _repository.Get(new Dictionary<string, object>()
            {
                { nameof(Borrowing.UserId), returnBook.UserId },
                { nameof(Borrowing.BookId), returnBook.BookId },
                { nameof(Borrowing.ReturnDate), null }
            }, 0, 1);

            if (data.TotalCount == 0)
                throw new ConflictException("You haven't borrowed this book.");

            await _repository.BeginTransaction(true);

            try
            {
                var borrowing = data.Data.First();
                borrowing.ReturnDate = returnBook.ReturnDate;
                book.AvailableCopies++;

                await _repository.Update(borrowing);
                await _bookRepo.Update(book);

                await _bookRepo.CommitTransaction();
            }
            catch
            {
                await _repository.RollbackTransaction();
                throw;
            }

        });

        if (!lockAcquired)
        {
            throw new ConflictException("Could not acquire lock, please try again later.");
        }
    });
}



    public static string GetBorrowKey(Guid bookId)
    {
        return $"Borrowing {bookId}";
    }
    public static string GetReturnKey(Guid bookId)
    {
        return $"Return {bookId}";
    }
}
