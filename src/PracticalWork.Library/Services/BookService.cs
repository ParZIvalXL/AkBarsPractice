using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Application.Interfaces;
using PracticalWork.Library.Data.Minio;
using PracticalWork.Library.Enums;
using PracticalWork.Library.Exceptions;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Services;

public sealed class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly IObjectStorage _objectStorage;
    private readonly ICacheService _cache;
    private readonly ICacheKeyGenerator _cacheKeyGenerator;
    private readonly ILogger<BookService> _logger;

    public BookService(
        IBookRepository bookRepository, 
        IObjectStorage objectStorage, 
        ICacheService cacheService,
        ICacheKeyGenerator cacheKeyGenerator,
        ILogger<BookService> logger)
    {
        _bookRepository = bookRepository;
        _objectStorage = objectStorage;
        _cache = cacheService;
        _cacheKeyGenerator = cacheKeyGenerator;
        _logger = logger;
    }

    public async Task<Guid> CreateBook(Book book)
    {
        ValidateBookForCreation(book);
        book.Status = BookStatus.Available;
        
        var id = await _bookRepository.CreateBook(book);
            
        await InvalidateBooksListCache();
        await InvalidateLibraryBooksCache();

        return id;
    }

    public async Task UpdateBook(Guid id, Book book)
    {
        var existingBook = await _bookRepository.GetBookById(id);
        
        if (existingBook.IsArchived)
            throw new BookServiceException("Нельзя редактировать архивированную книгу");
        
        if (existingBook.Category != book.Category)
            throw new BookServiceException("Категорию книги нельзя изменить");
        
        await _bookRepository.UpdateBook(id, book);
        
        await InvalidateBooksListCache();
        await _cache.RemoveAsync(_cacheKeyGenerator.GenerateBookDetailsKey(id));
        await InvalidateLibraryBooksCache();
    }

    public async Task ArchiveBook(Guid id)
    {
        var book = await _bookRepository.GetBookById(id);
        
        if (book.Status == BookStatus.Borrow)
            throw new BookServiceException("Нельзя архивировать выданную книгу");
        
        book.Archive();
        await _bookRepository.UpdateBook(id, book);
        
        await InvalidateBooksListCache();
        await InvalidateLibraryBooksCache();
        await _cache.RemoveAsync(_cacheKeyGenerator.GenerateBookDetailsKey(id));
    }

    public async Task<List<Book>> GetBooks(
        int? booksPerPage = 20,
        int? page = 1,
        BookCategory? category = null,
        [CanBeNull] string author = null,
        BookStatus? status = null)
    {
        if(page != null && booksPerPage != null)
            ValidateGetBooksParameters(booksPerPage, page);

        var cacheKey = _cacheKeyGenerator.GenerateBooksListKey(
            page, booksPerPage, category, author, status);
        
        return await _cache.GetOrSetAsync(cacheKey, async () =>
        {
            return await _bookRepository.GetBooks(booksPerPage, page, category, author, status);
        }, TimeSpan.FromMinutes(10));
    }

    /// <summary>
    /// Получение детальной информации о книге с кэшированием
    /// </summary>
    public async Task<Book> GetBookDetails(Guid id)
    {
        var cacheKey = _cacheKeyGenerator.GenerateBookDetailsKey(id);
        
        return await _cache.GetOrSetAsync(cacheKey, async () => await _bookRepository.GetBookById(id), TimeSpan.FromMinutes(30));
    }
    
    /// <summary>
    /// Добавление деталей для книги
    /// </summary>
    /// <param name="id">Индификатор</param>
    /// <param name="description">Описание</param>
    /// <param name="file">Обложка</param>
    /// <exception cref="BookServiceException">В случае если книга заархивирована</exception>
    public async Task AddDetails(Guid id, string description, byte[] file)
    {
        Book book = await _bookRepository.GetBookById(id);
    
        if(book.IsArchived)
            throw new BookServiceException("Книга архивирована!");

        ValidateImageFile(file);
        string extension = GetFileExtension(file);
        string fileName = $"book-covers/{book.Year}/{id}{extension}";
    
        using var stream = new MemoryStream(file);
        
        await _objectStorage.UploadFileAsync(fileName, stream);
    
        book.Description = description;
        book.CoverImagePath = fileName;
        
        _logger.LogInformation("Обложка книги: " +  book.CoverImagePath);
    
        await _bookRepository.UpdateBook(id, book);
        
        await _cache.RemoveAsync(_cacheKeyGenerator.GenerateBookDetailsKey(id));
        await InvalidateBooksListCache();
        await InvalidateLibraryBooksCache();
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

    private void ValidateImageFile(byte[] file)
    {
        const int maxFileSize = 5 * 1024 * 1024;
        
        if (file.Length > maxFileSize)
            throw new BookServiceException($"Размер файла не должен превышать 5MB");
        
        var extension = GetFileExtension(file);
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        
        if (!allowedExtensions.Contains(extension))
            throw new BookServiceException(
                $"Недопустимый формат изображения. Разрешены: {string.Join(", ", allowedExtensions)}");
    }

    private string GetFileExtension(byte[] file)
    {
        if (file.Length < 2) return ".jpg";
    
        if (file[0] == 0xFF && file[1] == 0xD8) return ".jpg";
        
        if (file.Length >= 4 && file[0] == 0x89 && file[1] == 0x50 && 
            file[2] == 0x4E && file[3] == 0x47) return ".png";
        
        if (file.Length >= 4 && file[0] == 0x52 && file[1] == 0x49 && 
            file[2] == 0x46 && file[3] == 0x46) return ".webp";
    
        return ".jpg";
    }

    /// <summary>
    /// Инвалидация кеша списка книг
    /// </summary>
    private async Task InvalidateBooksListCache()
    {
        await _cache.RemoveByPrefixAsync(_cacheKeyGenerator.BooksListPrefix);
    }

    /// <summary>
    /// Инвалидация кеша списка книг
    /// </summary>
    private async Task InvalidateLibraryBooksCache()
    {
        await _cache.RemoveByPrefixAsync(_cacheKeyGenerator.LibraryBooksPrefix);
    }
}