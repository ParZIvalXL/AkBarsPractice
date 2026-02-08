using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Application.Interfaces;
using PracticalWork.Library.Contracts.v2.Messaging;
using PracticalWork.Library.Contracts.v2.Readers.Events;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Services;

public class ReaderService : IReaderService
{   
    private readonly IReaderRepository _readerRepository;
    private readonly IBorrowRepository _borrowRepository;
    private readonly IBookService _bookService;
    private readonly ICacheService _cache;
    private readonly ICacheKeyGenerator _cacheKeyGenerator;
    private readonly IMessagePublisher _messagePublisher;
    
    public ReaderService(
        IReaderRepository readerRepository, 
        IBorrowRepository borrowRepository, 
        IBookService bookService,
        ICacheService cacheService,
        ICacheKeyGenerator cacheKeyGenerator,
        IMessagePublisher messagePublisher)
    {
        _readerRepository = readerRepository;
        _borrowRepository = borrowRepository;
        _bookService = bookService;
        _cache = cacheService;
        _cacheKeyGenerator = cacheKeyGenerator;
        _messagePublisher = messagePublisher;
    }
    
    public async Task<Guid> CreateReader(Reader reader)
    {
        reader.IsActive = true;
        var id = await _readerRepository.CreateReader(reader);

        await _cache.RemoveByPrefixAsync(_cacheKeyGenerator.ReadersListPrefix);

        var evt = new ReaderCreatedEvent(
            ReaderId: id,
            FullName: reader.FullName,
            PhoneNumber: reader.PhoneNumber,
            ExpiryDate: reader.ExpiryDate,
            CreatedAt: DateTime.UtcNow
        );
        await _messagePublisher.PublishAsync(evt);

        return id;
    }

    public async Task ExtendCard(Guid id, DateOnly newExpiryDate)
    {
        var reader = await GetReaderByIdInternal(id);
        
        if (reader == null)
            throw new ArgumentException("Не существует карточки с таким ID");
        
        if (!reader.IsActive)
            throw new ArgumentException("Карточка неактивна");
        
        var newExpiryUtc = newExpiryDate.ToDateTime(TimeOnly.MinValue).ToUniversalTime();
        var todayUtc = DateOnly.FromDateTime(DateTime.UtcNow).ToDateTime(TimeOnly.MinValue);

        if (newExpiryUtc < todayUtc)
            throw new ArgumentException(
                "Новая дата истечения срока действия не может быть меньше текущей даты");
        
        reader.ExpiryDate = newExpiryUtc;
        await _readerRepository.UpdateReader(id, reader);

        await _cache.RemoveAsync(_cacheKeyGenerator.GenerateReaderDetailsKey(id));
    }

    public async Task CloseCard(Guid id)
    {
        var reader = await GetReaderByIdInternal(id);
        
        if (reader == null)
            throw new ArgumentException("Не существует карточки с таким ID");
        
        if (!reader.IsActive)
            throw new ArgumentException("Карточка неактивна");
        
        var borrowedBooks = await _borrowRepository.GetReaderBorrowedBooks(id);
        if (borrowedBooks.Any())
            throw new ArgumentException($"У читателя есть книги, которые он еще не вернул: {string.Join(", ", borrowedBooks)}");
        
        reader.IsActive = false;
        reader.ExpiryDate = DateTime.UtcNow;

        await _readerRepository.UpdateReader(id, reader);
        await InvalidateReaderCache(id);

        var evt = new ReaderClosedEvent(
            ReaderId: id,
            FullName: reader.FullName,
            ClosedAt: DateTime.UtcNow,
            Reason: "Unknown"
        );
        await _messagePublisher.PublishAsync(evt);
    }

    public async Task<List<Guid>> GetReaderBooksIds(Guid id)
    {
        var reader = await GetReaderByIdInternal(id);
        if (reader == null)
            throw new ArgumentException("Не существует карточки с таким ID");

        return await _borrowRepository.GetReaderBorrowedBooks(id);
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
            var tasks = ids.Select(x => _bookService.GetBookById(x));
            return await Task.WhenAll(tasks);
        }, TimeSpan.FromMinutes(15));
    }

    public async Task<bool> IsNumberExistsInDatabase(string number)
    {
        var cacheKey = _cacheKeyGenerator.GeneratePhoneCheckKey(number);
        var cached = await _cache.GetAsync<bool?>(cacheKey);
        if (cached.HasValue) return cached.Value;

        var exists = await _readerRepository.IsPhoneNumberExistsInDatabase(number);
        await _cache.SetAsync(cacheKey, exists, TimeSpan.FromMinutes(1));
        return exists;
    }

    public async Task UpdateReader(Guid id, Reader reader)
    {
        var existingReader = await GetReaderByIdInternal(id);
        if (existingReader == null)
            throw new ArgumentException("Не существует карточки с таким ID");

        if (!existingReader.IsActive && reader.IsActive && reader.ExpiryDate < DateTime.UtcNow)
            throw new ArgumentException("Нельзя активировать карту с истекшим сроком действия");

        await _readerRepository.UpdateReader(id, reader);
        await InvalidateReaderCache(id);
    }

    private async Task<Reader> GetReaderByIdInternal(Guid id)
    {
        var cacheKey = _cacheKeyGenerator.GenerateReaderDetailsKey(id);
        return await _cache.GetOrSetAsync(cacheKey, () => _readerRepository.GetReaderById(id), TimeSpan.FromMinutes(30));
    }

    private async Task InvalidateReaderCache(Guid readerId)
    {
        await _cache.RemoveAsync(_cacheKeyGenerator.GenerateReaderDetailsKey(readerId));
        await _cache.RemoveAsync(_cacheKeyGenerator.GenerateReaderBooksKey(readerId));
        await _cache.RemoveByPrefixAsync(_cacheKeyGenerator.ReadersListPrefix);
    }
}
