using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Cache.Redis;
using PracticalWork.Library.Data.Minio;
using PracticalWork.Library.Enums;
using PracticalWork.Library.Exceptions;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Services;

public sealed class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly IObjectStorage _objectStorage;
    private readonly CacheService _cache;

    public BookService(IBookRepository bookRepository, IObjectStorage objectStorage, CacheService cacheService)
    {
        _bookRepository = bookRepository;
        _objectStorage = objectStorage;
        _cache = cacheService;
    }

    public async Task<Guid> CreateBook(Book book)
    {
        book.Status = BookStatus.Available;
        try
        {
            // TODO: глобальный обработчик ошибок
            var id = await _bookRepository.CreateBook(book);

            //await _cache.RemoveByPrefixAsync("books:list:");
            //await _cache.RemoveByPrefixAsync("library:books:");

            return id;
        }
        catch (Exception ex)
        {
            throw new BookServiceException("Ошибка создание книги!", ex);
        }
    }

    public async Task UpdateBook(Guid id, Book book)
    {
        await _bookRepository.UpdateBook(id, book);
        
        await _cache.RemoveByPrefixAsync("books:list:");
        await _cache.RemoveAsync(CacheKeys.BookDetails(id));
    }

    public async Task ArchiveBook(Guid id)
    {
        var book = await _bookRepository.GetBookById(id);
        book.Archive();
        await _bookRepository.UpdateBook(id, book);
        
        await _cache.RemoveByPrefixAsync("books:list:");
        await _cache.RemoveByPrefixAsync("library:books:");
    }
    
    public async Task<List<Book>> GetBooks(int booksPerPage, int page)
    {
        var books = await _bookRepository.GetBooks(booksPerPage, page);

        return books;
    }

    public async Task AddDetails(Guid id, string description, byte[] file)
    {
        Book book = await _bookRepository.GetBookById(id);
    
        if(book.IsArchived)
            throw new BookServiceException("Книга архивирована!");

        string extension = GetFileExtension(file);
        string fileName = $"books/{book.Title}/cover{extension}";
    
        using var stream = new MemoryStream(file);
        await _objectStorage.UploadFileAsync(fileName, stream);
    
        book.Description = description;
        book.CoverImagePath = fileName;
    
        await _bookRepository.UpdateBook(id, book);
    }

    private string GetFileExtension(byte[] file)
    {
        if (file.Length < 2) return ".jpg";
    
        if (file[0] == 0xFF && file[1] == 0xD8) return ".jpg";
        if (file.Length >= 3 && file[0] == 0x47 && file[1] == 0x49 && file[2] == 0x46) return ".gif";
        if (file.Length >= 4)
        {
            if (file[0] == 0x89 && file[1] == 0x50 && file[2] == 0x4E && file[3] == 0x47) return ".png";
            if (file[0] == 0x42 && file[1] == 0x4D) return ".bmp";
            if (file[0] == 0x52 && file[1] == 0x49 && file[2] == 0x46 && file[3] == 0x46) return ".webp";
        }
    
        return ".jpg";
    }
}