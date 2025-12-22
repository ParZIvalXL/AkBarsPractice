using Asp.Versioning;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Contracts.v1.Books.Request;
using PracticalWork.Library.Contracts.v1.Books.Response;
using PracticalWork.Library.Controllers.Mappers.v1;
using PracticalWork.Library.Enums;

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
    public async Task<IActionResult> GetBooks(BookStatus? status, BookCategory? category, [CanBeNull] string author, int? page, int? pageSize)
    {
        if (page < 1 )
            throw new ArgumentException("Страница указана неверно");
        if (pageSize < 1 || pageSize > 100)
            throw new  ArgumentException("Книг в странице не может быть меньше 1 или больше 100");
        
        var result = await _bookService.GetBooks(pageSize, page, category, author, status);
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
    /// <returns></returns>
    [HttpPost("/{id}/details")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> AddDetails([FromForm] AddBookDetailsRequest request, [FromRoute] Guid id)
    {
        if (request.Photo == null || request.Photo.Length == 0)
            return BadRequest("Файл обязателен");
        
        var allowedExtensions = new[] { ".jpeg", ".png", ".webp" };
        var fileExtension = Path.GetExtension(request.Photo.FileName).ToLowerInvariant();
    
        if (!allowedExtensions.Contains(fileExtension))
            return BadRequest($"Недопустимый формат файла. Разрешенные форматы: {string.Join(", ", allowedExtensions)}");
        
        if(request.Photo.Length > 5 * 1024 * 1024)
            return BadRequest("Файл должен весить не больше 5 МБ");
        
        await using (var memoryStream = new MemoryStream())
        {
            await request.Photo.CopyToAsync(memoryStream);
            var file = memoryStream.ToArray();
            
            await _bookService.AddDetails(id, request.Description, file);
        }

        return Ok();

    }
}