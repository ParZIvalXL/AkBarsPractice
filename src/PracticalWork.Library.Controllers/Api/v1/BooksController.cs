using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Contracts.v1.Books.Request;
using PracticalWork.Library.Contracts.v1.Books.Response;
using PracticalWork.Library.Controllers.Mappers.v1;

namespace PracticalWork.Library.Controllers.Api.v1;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/books")]
public class BooksController : Controller
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    /// <summary>
    /// Создание новой книги
    /// </summary>
    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CreateBookResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> CreateOrder(CreateBookRequest request)
    {
        var result = await _bookService.CreateBook(request.ToBook());


        return Content(result.ToString());
    }

    /// <summary>
    /// Обновление данных книги
    /// </summary>
    /// <param name="id">Идентификатор книги</param>
    /// <param name="request">Новые данные книги</param>
    [HttpPut("/{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> UpdateOrder([FromRoute] Guid id, UpdateBookRequest request)
    {
        await _bookService.UpdateBook(id, request.ToBook());
        // TODO: Redis
        return Ok();
    }

    /// <summary>
    /// Получение списка книг
    /// </summary>
    /// <returns>Книги</returns>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(GetBooksResponse), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetBooks()
    {
        var result = await _bookService.GetBooks(10, 1);
        // TODO: Redis
        return new JsonResult(result);
    }

    /// <summary>
    /// Архивировать книгу
    /// </summary>
    /// <param name="id">Идентификатор книги</param>
    [HttpPost("/{id}/archive")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> ArchiveBook([FromRoute] Guid id)
    {
        await _bookService.ArchiveBook(id);
        return Ok();
    }

    /// <summary>
    /// Добавление деталей книге
    /// </summary>
    /// <param name="id">Книга</param>
    /// <param name="photo">Обложка</param>
    /// <param name="request">Детали книги</param>
    /// <returns></returns>
    [HttpPost("/{id}/details")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> AddDetails(
        [FromRoute] Guid id,
        AddBookDetailsRequest request,
        IFormFile photo)
    {
        if (photo == null || photo.Length == 0)
            return BadRequest("Файл обязателен");
        
        if(photo.Length > 5 * 1024)
            return BadRequest("Файл должен весить не больше 5 МБ");
            
        await using (var memoryStream = new MemoryStream())
        {
            await photo.CopyToAsync(memoryStream);
            var file = memoryStream.ToArray();
            
            await _bookService.AddDetails(id, request.Description, file);
            // бакет исчез
            // TODO: путь к обложке
            
            
        }

        return Ok();

    }
}