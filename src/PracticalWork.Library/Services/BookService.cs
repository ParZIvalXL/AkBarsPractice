using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Application.Interfaces;
using PracticalWork.Library.Contracts.v2.Abstracts;
using PracticalWork.Library.Contracts.v2.Messaging;
using PracticalWork.Library.Contracts.v2.Readers.Events;
using PracticalWork.Library.Data.Minio;
using PracticalWork.Library.Enums;
using PracticalWork.Library.Exceptions;
using PracticalWork.Library.Models;
using PracticalWork.Reports.Domain.Enums;

namespace PracticalWork.Library.Services;

public sealed class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly IObjectStorage _objectStorage;
    private readonly ICacheService _cache;
    private readonly ICacheKeyGenerator _cacheKeyGenerator;
    private readonly ILogger<BookService> _logger;
    private readonly IMessagePublisher _messagePublisher;

    public BookService(
        IBookRepository bookRepository,
        IObjectStorage objectStorage,
        ICacheService cacheService,
        ICacheKeyGenerator cacheKeyGenerator,
        IMessagePublisher messagePublisher,
        ILogger<BookService> logger)
    {
        _bookRepository = bookRepository;
        _objectStorage = objectStorage;
        _cache = cacheService;
        _cacheKeyGenerator = cacheKeyGenerator;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }


    public async Task<Guid> CreateBook(Book book)
    {
        ValidateBookForCreation(book);
        book.Status = BookStatus.Available;
        
        var id = await _bookRepository.CreateBook(book);

        var evt = new BookCreatedEvent(
            id,
            book.Title,
            book.Category.ToString(),
            book.Authors.ToArray(),
            book.Year,
            DateTime.UtcNow);

        await _messagePublisher.PublishAsync(evt);

        return id;
    }

    public async Task UpdateBook(Guid id, Book book)
    {
        var existingBook = await _bookRepository.GetBookById(id);
        
        if (existingBook.IsArchived)
            throw new BookServiceException("Нельзя редактировать архивированную книгу");
        
        await _bookRepository.UpdateBook(id, book);
        
        await InvalidateBooksListCache();
        await _cache.RemoveAsync(_cacheKeyGenerator.GenerateBookDetailsKey(id));
        
        await _bookRepository.UpdateBook(id, book);

        await InvalidateBooksListCache();
        await _cache.RemoveAsync(_cacheKeyGenerator.GenerateBookDetailsKey(id));

    }

    public async Task ArchiveBook(Guid id)
    {
        var book = await _bookRepository.GetBookById(id);
        
        if (book.Status == BookStatus.Borrow)
            throw new BookServiceException("Нельзя архивировать выданную книгу");
        
        book.Archive();
        await _bookRepository.UpdateBook(id, book);

        var evt = new BookArchivedEvent(
            id,
            book.Title,
            "Unknown",
            DateTime.UtcNow);

        await _messagePublisher.PublishAsync(evt);

        await InvalidateBooksListCache();
        await _cache.RemoveAsync(_cacheKeyGenerator.GenerateBookDetailsKey(id));
    }

    public async Task<List<Book>> GetBooks(
        int? booksPerPage = 20,
        int? page = 1,
        BookCategory? category = null,
        [CanBeNull] string author = null,
        BookStatus? status = null,
        bool archivedFilter = false)
    {
        if(page != null && booksPerPage != null)
            ValidateGetBooksParameters(booksPerPage, page);

        var cacheKey = _cacheKeyGenerator.GenerateBooksListKey(
            page, booksPerPage, category, author, status);
        
        return await _cache.GetOrSetAsync(cacheKey, async () =>
        {
            return await _bookRepository.GetBooks(booksPerPage, page, category, author, status, archivedFilter);
        }, TimeSpan.FromMinutes(10));
    }

    public async Task<bool> BookTitleExists(string title)
    {
        return await _bookRepository.BookTitleExists(title);
    }

    public async Task<BookDetails> GetBookDetails(Guid id)
    {
        
        var cacheKey = _cacheKeyGenerator.GenerateBookDetailsKey(id);
        
        return await _cache.GetOrSetAsync(cacheKey, async () => await _bookRepository.GetBookDetails(id), TimeSpan.FromMinutes(30));
    }

    public async Task<BookDetails> GetBookDetails(string idOrTitle)
    {
        Guid id;
        if (await BookTitleExists(idOrTitle))
            id = await _bookRepository.GetBookIdByTitle(idOrTitle);

        else if (!Guid.TryParse(idOrTitle, out id))
            throw new ArgumentException("Не существует книги с таким названием или ID");
        
        var cacheKey = _cacheKeyGenerator.GenerateBookDetailsKey(id);
        
        return await _cache.GetOrSetAsync(cacheKey, async () => await _bookRepository.GetBookDetails(id), TimeSpan.FromMinutes(30));
    }

    
    /// <summary>
    /// Добавление деталей для книги
    /// </summary>
    /// <param name="id">Индификатор</param>
    /// <param name="description">Описание</param>
    /// <param name="file">Обложка</param>
    /// <param name="extension">Тип изображения</param>
    /// <exception cref="BookServiceException">В случае если книга заархивирована</exception>
    public async Task AddDetails(Guid id, string description, byte[] file, string extension)
    {
        Book book = await _bookRepository.GetBookById(id);
    
        if(book.IsArchived)
            throw new BookServiceException("Книга архивирована!");

        string fileName = $"book-covers/{book.Year}/{id}{extension}";
    
        using var stream = new MemoryStream(file);
        
        await _objectStorage.UploadFileAsync(fileName, stream);
    
        book.Description = description;
        book.CoverImagePath = fileName;
        
        _logger.LogInformation("Обложка книги: " +  book.CoverImagePath);
    
        await _bookRepository.UpdateBook(id, book);
        
        await _cache.RemoveAsync(_cacheKeyGenerator.GenerateBookDetailsKey(id));
        await InvalidateBooksListCache();
    }

    public async Task<Book> GetBookById(Guid id)
    {
        return await _bookRepository.GetBookById(id);
    }

    private void ValidateGetBooksParameters(int? booksPerPage, int? page)
    {
        if (page < 1)
            throw new BookServiceException("Номер страницы должен быть не меньше 1");
            
        if (booksPerPage < 1 || booksPerPage > 100)
            throw new BookServiceException("Размер страницы должен быть от 1 до 100");
    }

    private void ValidateBookForCreation(Book book)
    {
        if (book.Year > DateTime.Now.Year)
            throw new BookServiceException("Год издания не может быть в будущем");
            
        if (string.IsNullOrWhiteSpace(book.Title))
            throw new BookServiceException("Название книги обязательно");
            
        if (book.Authors == null || !book.Authors.Any())
            throw new BookServiceException("Книга должна иметь хотя бы одного автора");
            
        if (book.Category == BookCategory.Default)
            throw new BookServiceException("Категория книги должна быть указана");
    }


    /// <summary>
    /// Инвалидация кеша списка книг
    /// </summary>
    private async Task InvalidateBooksListCache()
    {
        await _cache.RemoveByPrefixAsync(_cacheKeyGenerator.BooksListPrefix);
    }
}