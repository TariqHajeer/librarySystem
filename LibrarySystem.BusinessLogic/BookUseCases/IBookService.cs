using System;
using LibrarySystem.BusinessLogic.BookUseCases.Dtos;
using LibrarySystem.BusinessLogic.Common;
using LibrarySystem.BusinessLogic.Domain;
using LibrarySystem.DataAccess.Helpers;

namespace LibrarySystem.BusinessLogic.BookUseCases;

public interface IBookService : IService<Book, Guid, CreateBook, BookListDto>
{
    Task<PagingResult<BookListDto>> GetBooks(string title, string author, string isbn,int page,int pageSize);
}
