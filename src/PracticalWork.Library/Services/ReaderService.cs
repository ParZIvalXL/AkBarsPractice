using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Application.Interfaces;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Services;

public class ReaderService : IReaderService
{   
    private readonly IReaderRepository _readerRepository;
    private readonly IBorrowRepository _borrowRepository;
    private readonly IBookService _bookService;
    private readonly ICacheService _cache;
    private readonly ICacheKeyGenerator _cacheKeyGenerator;
    
    public ReaderService(
        IReaderRepository readerRepository, 
        IBorrowRepository borrowRepository, 
        IBookService bookService,
        ICacheService cacheService,
        ICacheKeyGenerator cacheKeyGenerator)
    {
        _readerRepository = readerRepository;
        _borrowRepository = borrowRepository;
        _bookService = bookService;
        _cache = cacheService;
        _cacheKeyGenerator = cacheKeyGenerator;
    }
    
    public async Task<Guid> CreateReader(Reader reader)
    {
        if (string.IsNullOrWhiteSpace(reader.FullName))
            throw new ArgumentException("ФИО читателя обязательно");
        
        if (string.IsNullOrWhiteSpace(reader.PhoneNumber))
            throw new ArgumentException("Номер телефона обязателен");
        
        if (await IsNumberExistsInDatabase(reader.PhoneNumber))
            throw new ArgumentException("Читатель с таким номером телефона уже существует");
        
        var id = await _readerRepository.CreateReader(reader);

        await _cache.RemoveByPrefixAsync(_cacheKeyGenerator.ReadersListPrefix);

        return id;
    }

    public async Task ExtendCard(Guid id, DateOnly newExpiryDate)
    {
        var reader = await GetReaderByIdInternal(id);
        
        if(reader == null)
            throw new ArgumentException("Не существует карточки с таким ID");
        
        if(!reader.IsActive)
            throw new ArgumentException("Карточка неактивна");
        
        var newExpiryUtc = newExpiryDate.ToDateTime(TimeOnly.MinValue).ToUniversalTime();
        
        if(reader.ExpiryDate > newExpiryUtc)
            throw new ArgumentException("Новая дата истечения срока действия не может быть меньше текущей");
        
        reader.ExpiryDate = newExpiryUtc;
        
        await _readerRepository.UpdateReader(id, reader);
        
        await _cache.RemoveAsync(_cacheKeyGenerator.GenerateReaderDetailsKey(id));
    }

    public async Task CloseCard(Guid id)
    {
        var reader = await GetReaderByIdInternal(id);
        
        if(reader == null)
            throw new ArgumentException("Не существует карточки с таким ID");
        
        if(!reader.IsActive)
            throw new ArgumentException("Карточка неактивна");
        
        if(await _borrowRepository.IsReaderHasBorrowedBooks(id))
            throw new ArgumentException("У читателя есть книги которые он еще не вернул");
        
        reader.IsActive = false;
        
        await _readerRepository.UpdateReader(id, reader);
        
        await InvalidateReaderCache(id);
    }

    public async Task<List<Guid>> GetReaderBooksIds(Guid id)
    {
        var reader = await GetReaderByIdInternal(id);
        
        if(reader == null)
            throw new ArgumentException("Не существует карточки с таким ID");
        
        List<Guid> books = await _borrowRepository.GetReaderBorrowedBooks(id);
        
        return books;
    }

    public async Task<Book[]> GetReaderBooks(Guid id)
    {
        var reader = await GetReaderByIdInternal(id);
    
        if (reader == null)
            throw new ArgumentException("Не существует карточки с таким ID");
    
        var cacheKey = _cacheKeyGenerator.GenerateReaderBooksKey(id);
        
        return await _cache.GetOrSetAsync(cacheKey, async () =>
        {
            var ids = await GetReaderBooksIds(id);
    
            var bookTasks = ids.Select(x => _bookService.GetBookById(x));
    
            return await Task.WhenAll(bookTasks);
        }, TimeSpan.FromMinutes(15)); 
    }

    public async Task<bool> IsNumberExistsInDatabase(string number)
    {
        var cacheKey = _cacheKeyGenerator.GeneratePhoneCheckKey(number);
        var cached = await _cache.GetAsync<bool?>(cacheKey);
        
        if (cached.HasValue)
            return cached.Value;
        
        var exists = await _readerRepository.IsPhoneNumberExistsInDatabase(number);
        
        await _cache.SetAsync(cacheKey, exists, TimeSpan.FromMinutes(1));
        
        return exists;
    }

    public async Task UpdateReader(Guid id, Reader reader)
    {
        var existingReader = await GetReaderByIdInternal(id);
        
        if (existingReader == null)
            throw new ArgumentException("Не существует карточки с таким ID");
        
        if (!existingReader.IsActive && reader.IsActive)
        {
            if (reader.ExpiryDate < DateTime.UtcNow)
                throw new ArgumentException("Нельзя активировать карту с истекшим сроком действия");
        }
        
        await _readerRepository.UpdateReader(id, reader);
        
        await InvalidateReaderCache(id);
    }

    /// <summary>
    /// Внутренний метод для получения читателя с кэшированием
    /// </summary>
    private async Task<Reader> GetReaderByIdInternal(Guid id)
    {
        var cacheKey = _cacheKeyGenerator.GenerateReaderDetailsKey(id);
        
        return await _cache.GetOrSetAsync(cacheKey, async () => await _readerRepository.GetReaderById(id),
            TimeSpan.FromMinutes(30));
    }

    /// <summary>
    /// Инвалидация кэша читателя
    /// </summary>
    private async Task InvalidateReaderCache(Guid readerId)
    {
        await _cache.RemoveAsync(_cacheKeyGenerator.GenerateReaderDetailsKey(readerId));
        await _cache.RemoveAsync(_cacheKeyGenerator.GenerateReaderBooksKey(readerId));
        await _cache.RemoveByPrefixAsync(_cacheKeyGenerator.ReadersListPrefix);
    }
}