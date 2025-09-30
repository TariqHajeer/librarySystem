using LibrarySystem.BusinessLogic.BookUseCases;
using LibrarySystem.BusinessLogic.BorrowingUseCases;
using LibrarySystem.BusinessLogic.BorrowingUseCases.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;
        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }
        [HttpGet]
        public async Task<IActionResult> GetBooks([FromQuery] string? title, [FromQuery] string? author, [FromQuery] string? isbn, [FromQuery] int page, [FromQuery] int pageSize)
        {
            return Ok(await _bookService.GetBooks(title, author, isbn, page, pageSize));
        }
        [HttpPost("Borrow")]
        public async Task<IActionResult> Borrow(CreateBorrowing createBorrowing, [FromServices] IBorrowingService borrowingService)
        {
            return Ok(await borrowingService.Create(createBorrowing));
        }
        [HttpPost("ReturnBook")]
        public async Task<IActionResult> ReturnBook(ReturnBook returnBook, [FromServices] IBorrowingService borrowingService)
        {
            await borrowingService.ReturnBook(returnBook);
            return Ok();
        }
    }
}
