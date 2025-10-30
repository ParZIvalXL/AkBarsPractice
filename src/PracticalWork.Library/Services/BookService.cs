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
            var id = await _bookRepository.CreateBook(book);

            await _cache.RemoveByPrefixAsync("books:list:");
            await _cache.RemoveByPrefixAsync("library:books:");

            return id;
        }
        catch (Exception ex)
        {
            throw new BookServiceException("Ошибка создание книги!", ex);
        }
    }

    public async Task UpdateBook(Guid id, Book book)
    {
        try
        {
            await _bookRepository.UpdateBook(id, book);
            
            await _cache.RemoveByPrefixAsync("books:list:");
            await _cache.RemoveAsync(CacheKeys.BookDetails(id));
        }
        catch (Exception ex)
        {
            throw new BookServiceException("Ошибка обновления книги!", ex);
        }
    }

    public async Task ArchiveBook(Guid id)
    {
        try
        {
            await _bookRepository.ArchiveBook(id);
            await _cache.RemoveByPrefixAsync("books:list:");
            await _cache.RemoveByPrefixAsync("library:books:");
        }
        catch (Exception ex)
        {
            throw new BookServiceException("Ошибка архивации книги!", ex);
        }
    }

    public async Task<List<Book>> GetBooks()
    {
        var books = await _bookRepository.GetBooks();

        return books;
    }

    public async Task AddDetails(Guid id, byte[] file)
    {
        Book book = await _bookRepository.GetBookById(id);
        
        if (book == null)
            throw new BookServiceException("Книга не найдена!");
        
        await _objectStorage.UploadFileAsync(book.Title, new MemoryStream(file));
    }
}