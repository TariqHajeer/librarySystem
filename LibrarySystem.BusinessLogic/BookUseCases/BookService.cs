using LibrarySystem.BusinessLogic.BookUseCases.Dtos;
using LibrarySystem.BusinessLogic.Common;
using LibrarySystem.BusinessLogic.Domain;
using LibrarySystem.BusinessLogic.Repos.Interfaces;
using LibrarySystem.DataAccess.Exceptions;
using LibrarySystem.DataAccess.Helpers;

namespace LibrarySystem.BusinessLogic.BookUseCases;

internal class BookService : Service<Book, Guid, CreateBook, BookListDto>, IBookService
{
    public BookService(IRepository<Book, Guid> repository) : base(repository)
    {
    }

    public Task<PagingResult<BookListDto>> GetBooks(string title, string author, string isbn, int page, int pageSize)
    {
        // Create filter dictionary
        Dictionary<string, object> filter = new Dictionary<string, object>();

        if (!string.IsNullOrEmpty(title))
        {
            filter.Add(nameof(Book.Title), title);
        }

        if (!string.IsNullOrEmpty(author))
        {
            filter.Add(nameof(Book.Author), author);
        }

        if (!string.IsNullOrEmpty(isbn))
        {
            filter.Add(nameof(Book.ISBN), isbn);
        }

        // Calculate skip and take for pagination
        int skip = (page - 1) * pageSize;
        int take = pageSize;
        if (skip < 0 || take <= 0)
        {
            throw new ConflictException("there's problem with page and pagesize params please check");
        }

        // Call GetPage with filters and pagination
        return GetPage(filter, skip, take);
    }

}
