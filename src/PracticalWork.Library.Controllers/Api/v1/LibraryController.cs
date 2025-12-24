using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Contracts.v1.Library;
using PracticalWork.Library.Contracts.v1.Library.Request;

namespace PracticalWork.Library.Controllers.Api.v1;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/library")]
public class LibraryController : ControllerBase
{
    private readonly ILibraryService _libraryService;
    
    public LibraryController(ILibraryService libraryService)
    {
        _libraryService = libraryService;
    }
    
    [HttpPost("borrow")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> BorrowBook(BorrowBookRequest request)
    {
        Guid borrowId = await _libraryService.BorrowBook(request.ReaderId, request.BookId);
        return Ok(borrowId);
    }
    
    [HttpPost("return")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> ReturnBook(ReturnBookRequest request)
    {
        await _libraryService.ReturnBook(request.BookId);
        return Ok();
    }
    
    [HttpGet("books")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetBooks([FromQuery] GetBooksRequest request)
    {
        var result = await _libraryService.GetLibraryBooks(request.BookCategory == null? null : (int)request.BookCategory,
            request.Author, request.IsAvailable, request.BooksPerPage, request.Page);
        return Ok(result);
    }
    
    [HttpGet("books/{idOrTitle}/details")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetBookDetails([FromRoute] string idOrTitle)
    {
        var result = await _libraryService.GetBookDetails(idOrTitle);
        return Ok(result);
    }
}